﻿using myfoodapp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace myfoodapp.Business
{
    public sealed class MeasureBackgroundTask
    {
        private BackgroundWorker bw = new BackgroundWorker();
        private LogModel logModel = new LogModel();
        private DatabaseModel databaseModel = new DatabaseModel();
        private SensorManager.SensorManager sensorManager;


        public MeasureBackgroundTask()
        {
            logModel.AppendLog(Log.CreateLog("Measure Service starting...", Log.LogType.System));

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
        }

        public void Run()
        {
            logModel.AppendLog(Log.CreateLog("Measure Service running...", Log.LogType.System));
            sensorManager = SensorManager.SensorManager.GetInstance;
            bw.RunWorkerAsync();
        }

        public void Stop()
        {
            bw.CancelAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logModel.AppendLog(Log.CreateLog("Measure Service stopping...", Log.LogType.System));
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var watch = Stopwatch.StartNew();

            while (!bw.CancellationPending)
            {
                var elapsedMs = watch.ElapsedMilliseconds;
                var db = new LocalDataContext();

                if (elapsedMs % 10000 == 0)
                {
                    var oo = String.Empty;
                    var task = Task.Run(async () => { oo = await sensorManager.RecordPhTempMeasure(); });
                    task.Wait();
                    
                    try
                    {
                        var currentSensor = db.SensorTypes.Where(s => s.Id == 1).FirstOrDefault();
                        decimal captureValue = 0;
                        var capturedMeasure = Decimal.TryParse(oo.Replace("\r", ""), out captureValue);
                        db.Measures.Add(new Measure() { value = captureValue, captureDate = DateTime.Now, sensor = currentSensor });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }

            watch.Stop();      
        }
    }
}
