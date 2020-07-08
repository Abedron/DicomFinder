using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DicomFinder
{
    public class DicomDirectory
    {
        private static readonly Action EmptyDelegate = delegate { };

        private bool OneFileCheckBox { get; }
        private bool ValueCheckBox { get; }
        private DateTime ProgressTime { get; set; }

        public DicomDirectory(bool oneFileCheckBox, bool valueCheckBox)
        {
            this.OneFileCheckBox = oneFileCheckBox;
            this.ValueCheckBox = valueCheckBox;
        }

        public void Search(List<FileContext> files, string startDirectory, string group, string element, string valueBox, Label CountLabel)
        {
            ProgressTime = DateTime.Now;
            try
            {
                foreach (string path in Directory.GetFiles(startDirectory))
                {
                    var readData = DicomFileReader.Read(path, group, element);
                    string value = readData.Value;
                    if (value != null)
                    {
                        if (ValueCheckBox)
                        {
                            if (value.ToLower().Contains(valueBox.ToLower()))
                            {
                                files.Add(new FileContext(path, value, readData.Status));
                            }
                        }
                        else
                        {
                            files.Add(new FileContext(path, value, readData.Status));
                        }
                    }

                    var time = DateTime.Now - ProgressTime;
                    if (time.TotalMilliseconds > 1000)
                    {
                        ProgressTime = DateTime.Now + TimeSpan.FromMilliseconds(1000);

                        CountLabel.Content = "Count: " + files.Count;
                        CountLabel.Dispatcher.Invoke(DispatcherPriority.Background, EmptyDelegate);
                    }

                    if (OneFileCheckBox)
                    {
                        break;
                    }
                }

                foreach (string directory in Directory.GetDirectories(startDirectory))
                {
                    Search(files, directory, group, element, valueBox, CountLabel);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
