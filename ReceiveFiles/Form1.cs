using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Speech.Recognition;

namespace ReceiveFiles
{
    public partial class Form1 : Form
    {
        private const int port = 29251;
        private const int BufferSize = 1024;
        public string Status = string.Empty;
        public static bool completed = false;
        public static string txt;
        public Thread T = null;
        private static string result;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Server is Running... @ port: " + port.ToString();
            ThreadStart Ts = new ThreadStart(StartReceiving);
            T = new Thread(Ts);
            T.Start();

           
        }
        public void StartReceiving()
        {
            ReceiveTCP(port);
        }
        public void ReceiveTCP(int portN)
        {
            TcpListener Listener = null;
            try
            {
                Listener = new TcpListener(IPAddress.Any, portN);
                Listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            byte[] RecData = new byte[BufferSize];

            for (; ; )
            {
                TcpClient client = null;
                NetworkStream netstream = null;
                Status = string.Empty;
                try
                {
                    if (Listener.Pending())
                    {
                        client = Listener.AcceptTcpClient();
                        netstream = client.GetStream();
                        txt += DateTime.Now + ": Connected to a client\n";
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        int bytesRead = netstream.Read(buffer, 0, client.ReceiveBufferSize);

                        //---convert the data received into a string---
                        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received : " + dataReceived);

                        string SaveFileName = string.Empty;
                        SaveFileName = dataReceived;

                        ReadFromAudioFile(SaveFileName);

                        byte[] resultData = Encoding.ASCII.GetBytes(result);
                        int DataLength = resultData.Length;
                        netstream.Write(resultData, 0, DataLength);

                        netstream.Close();
                        client.Close();

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //netstream.Close();
                }
            }
        }
        private void ReadFromAudioFile(string saveFileName)
        {
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine())
            {

                // Create and load a grammar.  
                Grammar dictation = new DictationGrammar();
                dictation.Name = "Dictation Grammar";

                recognizer.LoadGrammar(dictation);

                // Configure the input to the recognizer.  
                recognizer.SetInputToWaveFile(saveFileName);


                // Attach event handlers for the results of recognition.  
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                recognizer.RecognizeCompleted +=
                  new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);

                // Perform recognition on the entire file.  
                txt += DateTime.Now + ": Starting asynchronous recognition...\n";
                completed = false;
                recognizer.Recognize();

            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }
        void updateTextbox(string txt)
        {
            Stats.Text += txt;
            
        }
        // Handle the SpeechRecognized event.  
        static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Text != null)
            {
                result = e.Result.Text;
                Console.WriteLine("  Recognized text =  {0}", e.Result.Text);
                txt += "Recognized Text is:\n";
                txt += e.Result.Text + "\n";

            }
            else
            {
                txt += DateTime.Now + " :Recognized text not available.\n";
            }
        }
        // Handle the RecognizeCompleted event.  
        static void recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                txt += DateTime.Now + " :  Error encountered." + e.Error.GetType().Name + ":" + e.Error.Message;
            }
            if (e.Cancelled)
            {
                txt += DateTime.Now + " :  Operation cancelled.\n";
            }
            if (e.InputStreamEnded)
            {
                txt += DateTime.Now + " :  End of stream encountered..\n";
            }
            completed = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            T.Abort();
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Stats.Text = txt;
        }
    }
}