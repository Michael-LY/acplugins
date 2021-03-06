﻿using acPlugins4net;
using acPlugins4net.helpers;
using acPlugins4net.kunos;
using acPlugins4net.messages;
using acServerFake.view.logviewer;
using System;
using System.Configuration;
using System.IO;
using acServerFake.viemodel.messages;

namespace acServerFake.viemodel
{
    public class ServerViewModel
    {
        public DuplexUDPClient UDPServer { get; set; }
        public RelayCommand OpenUDPConnection { get; set; }
        public static ServerViewModel Instance { get; set; }
        public CarInfoCollection CarInfoConfiguration { get; internal set; }

        public ServerViewModel()
        {
            UDPServer = new DuplexUDPClient();
            Instance = this;
        }

        public void Init()
        {
            var serverPort = Convert.ToInt32(ConfigurationManager.AppSettings["LOCAL_PORT"]);
            var pluginPort = Convert.ToInt32(ConfigurationManager.AppSettings["PLUGIN_PORT"]);

            OpenUDPConnection = new RelayCommand("Open UDP", (p) => 
            {
                UDPServer.Open(serverPort, "127.0.0.1", pluginPort, MessageReceived, OnError);
            });
        }

        internal void CloseUDPConnection()
        {
            UDPServer.Close();
        }

        private void OnError(Exception ex)
        {
            AwesomeViewerStolenFromTheInternet.LogException(ex);
            Console.WriteLine("Error: " + ex.Message);
        }

        public void SendMessage(PluginMessage msg)
        {
            var success = UDPServer.TrySend(msg.ToBinary());
            AwesomeViewerStolenFromTheInternet.Log(msg);
        }

        private void MessageReceived(TimestampedBytes data)
        {
            // The plugin did send us a Message, how nice :)
            // It will be raw data and needs to be decoded by the type (first byte)
            // Using the acPlugins4net library we can directly receive a PluginMessage
            var msg = AcMessageParser.Parse(data);
            AwesomeViewerStolenFromTheInternet.Log(msg);

            // if there is a Request, we'll have to send the corresponding answer.
            if(msg.Type == ACSProtocol.MessageType.ACSP_GET_CAR_INFO)
            {
                var request = msg as RequestCarInfo;
                var response = CarInfoConfiguration.GetMessage(request.CarId);
                UDPServer.TrySend(response.ToBinary());
            }

            /*
            // Currently we only have two very simple messages, so we're do it here:
            try
            { 
                using (var br = new BinaryReader(new MemoryStream(data)))
            {
                var msgType = (ACSProtocol.MessageType)br.ReadByte();
                switch (msgType)
                {
                    case ACSProtocol.MessageType.ACSP_GET_CAR_INFO:
                        {
                            // The plugin asks for the car_info of car_id X
                            var requested_car_id = br.ReadByte();

                            // Then it expects a single answer
                            var carInfoMsg = new MsgCarInfo() { CarId = requested_car_id, CarModel = "bmw_e300", CarSkin = "anthrazit", DriverName = "Minolin", DriverTeam = "Team", DriverGuid = "2468274569", IsConnected = true };
                            UDPServer.TrySend(carInfoMsg);
                        }
                        break;
                    case ACSProtocol.MessageType.ACSP_REALTIMEPOS_INTERVAL:
                        {
                        }
                        break;
                    default:
                        throw new Exception("Unknown/unexpected incoming message type '" + msgType + "'");
                }
            }
                }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }*/
        }
    }
}
