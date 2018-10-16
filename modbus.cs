using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace ASU_KV_001
{
    public class ModBus_Class
    {
        public SerialDevice serialport = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        public ObservableCollection<DeviceInformation> listOfDevices;
        public CancellationTokenSource ReadCancellationTokenSource;
        public ListBox devices;
        public string status;
        public bool error;

        public char adress;
        public char baud;

        private byte rd_try=0;
        public byte rd_mode=0;
        public byte wait_answer=0;
        public float weight;
        public short in_outs;
        public short q_state;
        public float sum_last;
        public uint count_last;
        public float[] doza_last = new float[3];
        public byte last_flag = 0;

        public float[] doze = new float[6];

        public float reg_fl;
        public float write_reg_fl;
        public UInt32 reg_long;
        public UInt32 write_reg_long;
        public uint reg_int;
        public uint write_reg_int;
        public byte write_command;
        public byte term_adress;
        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
        public async Task ListAvailablePorts()
        {
            error = false;
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                status = "СТАТУС: ВЫБЕРИТЕ И ПОДКЛЮЧИТЕ УСТРОЙСТВО";
                listOfDevices = new ObservableCollection<DeviceInformation>();
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

             //   DeviceListSource.Source = listOfDevices;
            }
            catch (Exception ex)
            {
                status = "СТАТУС: "+ex.Message;
                error = true;
            }
        }
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>

        public async Task OpenDevices( ListBox devices_list)
        {
            var selection = devices_list.SelectedItems;
            error = false; 
            if (selection.Count <= 0)
            {
                error = true;
                status = "СТАТУС: ВЫБЕРИТЕ И ПОДКЛЮЧИТЕ УСТРОЙСТВО";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];

            try
            {
                serialport = await SerialDevice.FromIdAsync(entry.Id);


                // Configure serial settings
                serialport.WriteTimeout = TimeSpan.FromMilliseconds(50);
                serialport.ReadTimeout = TimeSpan.FromMilliseconds(50);
                serialport.BaudRate = 9600;
                serialport.Parity = SerialParity.None;
                serialport.StopBits = SerialStopBitCount.One;
                serialport.DataBits = 8;
                serialport.Handshake = SerialHandshake.None;

                // Display configured settings
                status = "СТАТУС: ПОДКЛЮЧЕНО - ";
                status += serialport.BaudRate + "-";
                status += serialport.DataBits + "-";
                status += serialport.Parity.ToString() + "-";
                status += serialport.StopBits;

                wait_answer = 0;


                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                // Enable 'WRITE' button to allow sending data
//                sendTextButton.IsEnabled = true;

                Listen();
            }
            catch (Exception ex)
            {
                status = "СТАТУС: " + ex.Message;
                error = true;
            }

        }
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteAsync()
        {
            Task<UInt32> storeAsyncTask;
       //     string sendText="TEST";
            byte[] array = { 0x01, 0x03, 0x00, 0x01, 0x00,0x02,0x95,0xcb}; ;
            if ((rd_try==0))
            {
                // Load the text from the sendText input text box to the dataWriter object
                //                dataWriteObject.WriteString(sendText);
                //  dataWriteObject.WriteBytes(array);
                switch (rd_mode)
                {
                    case 1: ReadStateData(term_adress); break;
                    case 2: ReadFloat(term_adress, 25); break;
                    case 3: ReadFloat(term_adress, 29); break;
                    case 4: ReadFloat(term_adress, 33); break;

                    case 8: ReadLastData(term_adress); break;




                    case 20: ReadFloat(term_adress, 25); break;  //Доза 1
                    case 21: ReadFloat(term_adress, 27); break;//Доза 1  -2 коомп
                    case 22: ReadFloat(term_adress, 29); break;  //Доза 2
                    case 23: ReadFloat(term_adress, 31); break;//Доза 2  -2 коомп
                    case 24: ReadFloat(term_adress, 33); break; //Доза 3
                    case 25: ReadFloat(term_adress, 35); break;//Доза 3  -2 коомп
                    case 40: WriteFloat(term_adress, 25); break;
                    case 41: WriteFloat(term_adress, 27); break;
                    case 42: WriteFloat(term_adress, 29); break;
                    case 43: WriteFloat(term_adress, 31); break;
                    case 44: WriteFloat(term_adress, 33); break;
                    case 45: WriteFloat(term_adress, 35); break;
                    case 60: ReadFloat(term_adress, 37); break;
                    case 61: ReadFloat(term_adress, 39); break;
                    case 62: ReadFloat(term_adress, 41); break;
                    case 63: ReadFloat(term_adress, 43); break;
                    case 64: ReadFloat(term_adress, 45); break;
                    case 65: ReadFloat(term_adress, 47); break;
                    case 66: ReadFloat(term_adress, 49); break;
                    case 67: ReadFloat(term_adress, 51); break;
                    case 68: ReadFloat(term_adress, 53); break;
                    case 69: ReadFloat(term_adress, 55); break;
                    case 70: ReadFloat(term_adress, 57); break;
                    case 71: ReadFloat(term_adress, 59); break;
                    case 80: WriteFloat(term_adress, 37); break;
                    case 81: WriteFloat(term_adress, 39); break;
                    case 82: WriteFloat(term_adress, 41); break;
                    case 83: WriteFloat(term_adress, 43); break;
                    case 84: WriteFloat(term_adress, 45); break;
                    case 85: WriteFloat(term_adress, 47); break;
                    case 86: WriteFloat(term_adress, 49); break;
                    case 87: WriteFloat(term_adress, 51); break;
                    case 88: WriteFloat(term_adress, 53); break;
                    case 89: WriteFloat(term_adress, 55); break;
                    case 90: WriteFloat(term_adress, 57); break;
                    case 91: WriteFloat(term_adress, 59); break;

                    case 100: ReadFloat(term_adress, 70); break;
                    case 101: ReadFloat(term_adress, 77); break; //Добавить считыываени время успокоени
                    case 102: ReadUInt(term_adress, 74); break;
                    case 103: ReadUInt(term_adress, 75); break;
                    case 104: ReadUInt(term_adress, 76); break;
                    case 120: WriteFloat(term_adress, 70); break;
                    case 121: WriteFloat(term_adress, 59); break;
                    case 122: WriteUInt(term_adress, 74); break;
                    case 123: WriteUInt(term_adress, 75); break;
                    case 124: WriteUInt(term_adress, 76); break;
                    case 140: ReadUInt(term_adress, 4); break;
                    case 141: ReadUInt(term_adress, 5); break;
                    case 142: ReadFloat(term_adress, 6); break;
                    case 143: ReadFloat(term_adress, 8); break;
                    case 144: ReadFloat(term_adress, 10); break;
                    case 145: ReadULong(term_adress, 12); break;
                    case 146: ReadUInt(term_adress, 14); break;
                    case 160: WriteUInt(term_adress, 4); break;
                    case 161: WriteUInt(term_adress, 5); break;
                    case 162: WriteFloat(term_adress, 6); break;
                    case 163: WriteFloat(term_adress, 8); break;
                    case 164: WriteFloat(term_adress, 10); break;
                    case 165: WriteULong(term_adress, 12); break;
                    case 166: WriteUInt(term_adress, 14); break;
                    case 180: ReadUInt(term_adress, 15); break;
                    case 181: ReadUInt(term_adress, 16); break;
                    case 182: ReadFloat(term_adress, 10); break;
                    case 183: ReadUInt(term_adress, 14); break;

                        //                    case 200: WriteFloat(term_adress, 14); break;
                        //                    case 201: WriteFloat(term_adress, 18); break;
                        //                    case 202: WriteFloat(term_adress, 22); break;

                        /*     case 22: ReadUInt(0x01, 44); break;
                             case 23: ReadUInt(0x01, 45); break;
                             case 24: ReadUInt(0x01, 46); break;
                             case 25: ReadUInt(0x01, 47); break;
                             case 40: ReadUInt(0x01, 4); break;
                             case 41: ReadUInt(0x01, 5); break;
                             case 42: ReadUInt(0x01, 6); break;
                             case 43: ReadFloat(0x01, 7); break;
                             case 44: ReadFloat(0x01, 9); break;
                             case 45: ReadFloat(0x01, 11); break;
                             case 46: ReadULong(0x01, 13); break;
                             case 60: ReadFloat(0x01, 25); break;
                             case 61: ReadFloat(0x01, 27); break;
                             case 62: ReadFloat(0x01, 29); break;
                             case 63: ReadFloat(0x01, 31); break;
                             case 70: WriteFloat(0x01, 25); break;
                             case 71: WriteFloat(0x01, 27); break;
                             case 72: WriteFloat(0x01, 29); break;
                             case 73: WriteFloat(0x01, 31); break;
                             case 80: WriteUInt(0x01, 4); break;
                             case 81: WriteUInt(0x01, 5); break;
                             case 82: WriteUInt(0x01, 6); break;
                             case 83: WriteFloat(0x01, 7); break;
                             case 84: WriteFloat(0x01, 9); break;
                             case 85: WriteFloat(0x01, 11); break;
                             case 86: WriteULong(0x01, 13); break;
                             case 100: WriteFloat(0x01, 40); break;
                             case 101: WriteFloat(0x01, 42); break;
                             case 102: WriteUInt(0x01, 44); break;
                             case 103: WriteUInt(0x01, 45); break;
                             case 104: WriteUInt(0x01, 46); break;
                             case 105: WriteUInt(0x01, 47); break;
                             case 200: 
                             case 201: 
                             case 202: WriteCommand(0x01, write_command); break;*/

                }
                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                rd_try = 3;
                UInt32 bytesWritten = await storeAsyncTask;
            /*    if (bytesWritten > 0)
                {
                    status = "СТАТУС: "+bytesWritten+"байт отправлено!";
                }
                */
            }
            else
            {
                rd_try--;
            }
        }
        
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialport != null)
                {
                    dataReaderObject = new DataReader(serialport.InputStream);

                    

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    status = "Reading task was cancelled, closing device and cleaning up";
                    CloseDevice();
                }
                else
                {
                    status = "Ошибка: "+ex.Message;
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B,0x00,0x00 };
            uint ReadBufferLength = 1024;
            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                rd_try = 0;
                wait_answer = 2;
                // rcvdText.Text = dataReaderObject.ReadString(bytesRead);
                byte[] fileContent = new byte[dataReaderObject.UnconsumedBufferLength];
                dataReaderObject.ReadBytes(fileContent);
                string text = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                switch (rd_mode)
                {
                    case 1: weight = BitConverter.ToSingle(fileContent, 3);
                            q_state= BitConverter.ToInt16(fileContent, 9);
                        break;
                    case 2:
                        doze[0] = BitConverter.ToSingle(fileContent, 3);
                        break;
                    case 3:
                        doze[1] = BitConverter.ToSingle(fileContent, 3);
                        break;
                    case 4:
                        doze[2] = BitConverter.ToSingle(fileContent, 3);
                        break;

                    //                    case 2: reg_int = BitConverter.ToInt16(fileContent, 3); break;
                    //                    case 3: reg_int = BitConverter.ToInt16(fileContent, 3); break;
                    case 5: reg_fl = BitConverter.ToSingle(fileContent, 3); break;
                    case 8: sum_last = BitConverter.ToSingle(fileContent, 3);
                            count_last = BitConverter.ToUInt32(fileContent, 7);
                        doza_last[0] = BitConverter.ToSingle(fileContent, 11);
                        doza_last[1] = BitConverter.ToSingle(fileContent, 15);
                        doza_last[2] = BitConverter.ToSingle(fileContent, 19);
                        last_flag = 1;
                        break;
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 100:
                    case 101:
                    case 142:
                    case 143:
                    case 144:

                        reg_fl = BitConverter.ToSingle(fileContent, 3); break;
                    case 102:
                    case 103:
                    case 104:
                    case 140:
                    case 141:
                        reg_int = BitConverter.ToUInt16(fileContent, 3); break;
                    case 145:
                        reg_long = BitConverter.ToUInt32(fileContent, 3); break;
                        /*case 40:
                        case 41:
                        case 42:
                            reg_int = BitConverter.ToUInt16(fileContent, 3); break;
                        case 43:
                        case 44:
                        case 45:
                            reg_fl = BitConverter.ToSingle(fileContent, 3); break;
                        case 46:
                            reg_long = BitConverter.ToUInt32(fileContent, 3); break;
                        case 60:
                        case 61:
                        case 62:
                        case 63:
                            reg_fl = BitConverter.ToSingle(fileContent, 3); break;

        */
                }
                status = ""+ bytesRead+"bytes read successfully! ";
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        public void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        public void CloseDevice()
        {
            if (serialport != null)
            {
                serialport.Dispose();
            }
            serialport = null;

            listOfDevices.Clear();
        }
        public async Task ClosePort()
        {
            try
            {
                status = "";
                CancelReadTask();
                CloseDevice();
                await ListAvailablePorts();
            }
            catch (Exception ex)
            {
               status = "СТАТУС: "+ex.Message;
            }
        }

        public async Task WriteToPort()
        {
            try
            {
                if (serialport != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialport.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteAsync();
                }
                else
                {
                    status = "СТАТУС: ВЫБЕРИТЕ И ПОДКЛЮЧИТЕ УСТРОЙСТВО";
                }
            }
            catch (Exception ex)
            {
                status = "СТАТУС: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        private void ReadStateData(byte TermNum)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(0);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x04;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
            // WRITE(ch, 8);


        }
        private void ReadDozes(byte TermNum)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(0);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x14;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
            // WRITE(ch, 8);


        }
        private void ReadLastData(byte TermNum)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(95);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x10;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
            // WRITE(ch, 8);


        }
        private void ReadFloat(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x02;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
        }
        private void WriteFloat(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x10, 0xAA, 0xAA, 0x00, 0x02, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x10;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x02;
            ch[6] = 0x04;
            byteArray = BitConverter.GetBytes(write_reg_fl);
            ch[7] = byteArray[0];
            ch[8] = byteArray[1];
            ch[9] = byteArray[2];
            ch[10] = byteArray[3];
            byteArray = BitConverter.GetBytes(CRC16(ch, 11, 0));
            ch[11] = byteArray[0]; ch[12] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;


        }
        private void WriteULong(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x10, 0xAA, 0xAA, 0x00, 0x02, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x10;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x02;
            ch[6] = 0x04;
            byteArray = BitConverter.GetBytes(write_reg_long);
            ch[7] = byteArray[0];
            ch[8] = byteArray[1];
            ch[9] = byteArray[2];
            ch[10] = byteArray[3];
            byteArray = BitConverter.GetBytes(CRC16(ch, 11, 0));
            ch[11] = byteArray[0]; ch[12] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
        }
        private void WriteUInt(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x10, 0xAA, 0xAA, 0x00, 0x02, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x10;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x01;
            ch[6] = 0x02;
            byteArray = BitConverter.GetBytes(write_reg_int);
            ch[7] = byteArray[0];
            ch[8] = byteArray[1];
            byteArray = BitConverter.GetBytes(CRC16(ch, 9, 0));
            ch[9] = byteArray[0]; ch[10] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;


        }
        private void ReadULong(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x02;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
        }
        private void ReadUInt(byte TermNum, uint Adr)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = BitConverter.GetBytes(Adr);

            ch[0] = TermNum;
            ch[1] = 0x03;
            ch[2] = byteArray[1];
            ch[3] = byteArray[0];
            ch[4] = 0x00;
            ch[5] = 0x01;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;
            // WRITE(ch, 8);

        }

        void WriteCommand(byte TermNum, byte Comnd)
        {
            byte[] ch = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x02, 0xC4, 0x0B };
            byte[] byteArray = { 0x00, 0x00 };
            ch[0] = TermNum;
            ch[1] = 0x06;
            ch[2] = 0x00;
            ch[3] = 0x00;
            ch[4] = 0x00;
            ch[5] = Comnd;
            byteArray = BitConverter.GetBytes(CRC16(ch, 6, 0));
            ch[6] = byteArray[0]; ch[7] = byteArray[1];
            dataWriteObject.WriteBytes(ch);
            wait_answer = 1;

        }
        public static ushort CRC16(byte[] data, int length, int offset)
        {
            byte[] aucCRCHi = {
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40
            };

            byte[] aucCRCLo = {
                0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
                0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
                0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
                0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
                0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
                0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
                0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
                0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
                0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
                0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
                0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
                0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
                0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
                0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
                0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
                0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
                0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
                0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
                0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
                0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
                0x41, 0x81, 0x80, 0x40
            };

            if (offset >= length)
                throw new ArgumentOutOfRangeException();

            byte ucCRCHi = 0xFF;
            byte ucCRCLo = 0xFF;
            int idx;

            int i = offset;
            int len = length;
            while (len-- > 0)
            {
                idx = ucCRCLo ^ data[i++];
                ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[idx]);
                ucCRCHi = aucCRCLo[idx];
            }

            return (ushort)(ucCRCHi << 8 | ucCRCLo);
        }

    }
}
