using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;

namespace WoodGrain
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int FrameHeigth = 900;
        private int layercout = 0;
        private string gcode;
        private List<int> temps = new List<int>();
        private string filePath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            button_preview.IsEnabled = false;
            button_process.IsEnabled = false;
            button_write.IsEnabled = false;
            progressbar_file.Visibility = Visibility.Hidden;
        }

        private void button_openFile_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "gcode files (*.gcode)|*.gcode";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            else
                return;

            gcode = File.ReadAllText(filePath,Encoding.ASCII);
            textBox_gcode.Text = gcode.Substring(0, 100000);
            string a = gcode.Substring(gcode.IndexOf("LAYER_COUNT") + 12);
            int index = a.IndexOf('\r');
            string b = a.Substring(0, index);
            layercout = int.Parse(b);
            label_layercount.Content = "Layers found: " + layercout.ToString();

            int strlenght = gcode.Length;
            progressbar_file.Visibility = Visibility.Visible;
            while (strlenght != gcode.Length)
            {
                strlenght = gcode.Length;
                AllowUIToUpdate();
                progressbar_file.IsIndeterminate = true;
            }

            progressbar_file.Visibility = Visibility.Hidden;
            progressbar_file.IsIndeterminate = false;

            button_preview.IsEnabled = true;
        }

        private void button_preview_Click(object sender, RoutedEventArgs e)
        {
            stackpannel_preview.Children.Clear();
            temps.Clear();
            List<Line> previewLines = new List<Line>();
            List<int> startLayer = new List<int>();
            List<int> startLayer2 = new List<int>();
            Random r;
            if (!string.IsNullOrEmpty(textbox_seed.Text))
            {
                try
                {
                    int a = int.Parse(textbox_seed.Text);
                    if (a < 0)
                        throw new OverflowException();
                    textbox_seedDisplay.Text = textbox_seed.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("This Seed is either to big or negativ and therefore not valid");
                    return;
                }
                r = new Random(int.Parse(textbox_seed.Text));
            }
            else
            {
                Random seedgen = new Random();
                int seed = seedgen.Next(0, Int32.MaxValue);
                r = new Random(seed);
                textbox_seedDisplay.Text = seed.ToString();
            }
            int linesPer100Layers = r.Next(2, 10);
            int lines = (int)(linesPer100Layers * (double)layercout / (double)100);
            for(int i = 0; i < lines; i++)
                startLayer.Add(r.Next(1, 10));

            int startLayersum = startLayer.Sum();
            for (int i = 0; i < startLayer.Count; i++)
                startLayer[i] = (int)((double)startLayer[i] / startLayersum * layercout);


            for (int i = 0; i < startLayer.Count; i++)
                startLayer2.Add(startLayer.Take(i+1).Sum());

            int j = 0;
            bool up = false;
            int min = 0;
            int max = 0;
            try
            {
                min = int.Parse(textbox_minTemp.Text);
                max = int.Parse(textbox_maxTemp.Text);
            }catch(Exception ex)
            { MessageBox.Show("Only enter valid INTs"); return; }
            for(int i = 0; i < layercout; i++)
            {
                if(i == startLayer2[j])
                {
                    if (up)
                        up = false;
                    else
                        up = true;
                    j++;
                    if (j >= startLayer2.Count - 1)
                        j = startLayer2.Count - 1;
                }
                int temp;
                int index = j - 1;
                if (index < 0)
                    index = 0;
                if (up)
                     temp = min + ((i - startLayer2[index]) * 1);
                else
                     temp = max - ((i - startLayer2[index]) * 1);

                if (temp < min)
                    temp = min;
                else if (temp > max)
                    temp = max;

                temps.Add(temp);

                button_process.IsEnabled = true;
            }

            for (int i = 0; i < FrameHeigth; i++)
            {
                int index = (int)((i / (double)FrameHeigth) * layercout);
                Color c = Color.FromRgb((byte)temps[index], (byte)temps[index], (byte)temps[index]);
                previewLines.Add(new Line { Stroke = new SolidColorBrush { Color = c }, X1 = 0, X2 = 100, Y1 = 0, Y2 = 0, StrokeThickness = 1 });
            }

            foreach (Line l in previewLines)
                stackpannel_preview.Children.Add(l);
        }

        private void button_process_Click(object sender, RoutedEventArgs e)
        {
            progressbar_file.Visibility = Visibility.Visible;
            progressbar_file.Value = 0;
            gcode = File.ReadAllText(filePath, Encoding.ASCII);
            textBox_gcode.Clear();
            int index = 0;
            int offset = 0;
            for(int i = 0; i < layercout; i++)
            {
                progressbar_file.Value = (int)((double)i / layercout * 100);
                index = gcode.IndexOf(";LAYER:", index);
                offset = gcode.IndexOf("\r\n", index);
                string a = gcode.Substring(index, offset - index + 2);
                string b = gcode.Substring(index + offset - index + 2, 10);
                string insert = "M104 S" + temps[i].ToString() + "\r\n";
                gcode = gcode.Insert(index + offset - index + 2, insert);
                index += insert.Length;
                AllowUIToUpdate();
            }
            textBox_gcode.Text = gcode.Substring(0, 100000);
            button_write.IsEnabled = true;
            progressbar_file.Visibility = Visibility.Hidden;
        }

        private void button_write_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.InitialDirectory = "c:\\";
            saveFileDialog.Filter = "gcode files (*.gcode)|*.gcode";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            string savePath = string.Empty;

            if (saveFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                savePath = saveFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = saveFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            else
                return;

            File.WriteAllText(savePath, gcode);
        }

        void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }
    }
}
