﻿using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Data.Entity.Migrations;
using System.Data;

namespace MedLab.Windows.LaboratoryAssistantResearcherWindows
{
    /// <summary>
    /// Логика взаимодействия для MainLaboratoryAssistantResearcher.xaml
    /// </summary>
    public partial class MainLaboratoryAssistantResearcher : Window
    {
        private MainWindow _mainWindow;
        private History _newHistory;
        private User _users;

        public MainLaboratoryAssistantResearcher(MainWindow mainWindow, History History, User User)
        {
            InitializeComponent();
            try
            {
                _mainWindow = mainWindow;
                _newHistory = History;
                _users = User;
                if (User != null)
                {
                    StartTimer();
                    FIOUser.Text = User.Surname + " " + User.Name + " " + User.Patronymic;
                }
                else
                {
                    MessageBox.Show($"Ошибка");
                    NewHistory(3);
                    _mainWindow.Show();
                    this.Close();
                }
                var listAnalyzers = medLabEntities.Analyzers.ToList();
                foreach (var item in listAnalyzers)
                {
                    ListAnalyzer.Items.Add(item.AnalyzerName);
                }
                ListAnalyzer.Items.Add("Biorad/Ledetect");
                List<string> serviceSearchBy = new List<string>
                {
                    "Номер",
                    "Название",
                    "Статус",
                };
                foreach (var item in serviceSearchBy)
                {
                    ServiceSearchBy.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private int remainingSeconds;
        private Timer timer;
        private bool checkClosing = false;
        MedLabEntities medLabEntities = new MedLabEntities();
        private void StartTimer()
        {
            remainingSeconds = 9000;
            bool fifteenMinutesAlerted = false;
            timer = new Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                if (remainingSeconds > 0)
                {
                    remainingSeconds--;
                    int hours = remainingSeconds / 3600;
                    int minutes = (remainingSeconds % 3600) / 60;
                    int seconds = remainingSeconds % 60;
                    string timeRemaining = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

                    Dispatcher.Invoke(() =>
                    {
                        timerTB.Text = timeRemaining;
                        if (remainingSeconds < 300 && !fifteenMinutesAlerted)
                        {
                            fifteenMinutesAlerted = true;
                            timerTB.Foreground = new SolidColorBrush(Colors.Red);
                            MessageBox.Show("Осталось 5 минут");
                        }
                    });
                }
                else
                {
                    timer.Stop();
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Время вышло!");
                        NewHistory(2);
                        checkClosing = true;
                        _mainWindow.Show();
                        this.Close();
                    });
                }
            };
            timer.Start();
        }

        public void NewHistory(int reason)
        {
            _newHistory.DateEnd = DateTime.Now;
            _newHistory.IdReason = reason;
            medLabEntities.Histories.Add(_newHistory);
            medLabEntities.SaveChanges();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
            if (!checkClosing)
            {
                NewHistory(1);
            }
        }

        private void ExitMainForLabAssitantRes_Click(object sender, RoutedEventArgs e)
        {
            checkClosing = true;
            NewHistory(1);
            _mainWindow.Show();
            this.Close();
        }

        private string serviceSearchBy;


        private void ServiceSearchText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(serviceSearchBy))
            {
                ListOutstandingService.ItemsSource = null;
                switch (serviceSearchBy)
                {
                    case "Номер":
                        if (selectedIdTypeAnalyzer == 3)
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == 1 && x.Service.AnalyzerType == 2 
                                && (x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7) && x.IdOrder.ToString()
                                .StartsWith(ServiceSearchText.Text))
                                .ToList();
                        }
                        else
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                                .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7).Where(x => x.IdOrder.ToString()
                                .StartsWith(ServiceSearchText.Text)).ToList();
                        } 
                        break;
                    case "Название":
                        if (selectedIdTypeAnalyzer == 3)
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == 1 && x.Service.AnalyzerType == 2
                                && (x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                                && x.Service.ServiceName.StartsWith(ServiceSearchText.Text))
                                .ToList();
                        }
                        else
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                                .Where(x=>x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7).Where(x => x.Service.ServiceName
                                .StartsWith(ServiceSearchText.Text))
                                .ToList();
                        }
                        break;
                    case "Статус":
                        if (selectedIdTypeAnalyzer == 3)
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == 1 && x.Service.AnalyzerType == 2
                                && (x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7) && x.StatusServicesInOrder.Status
                                .StartsWith(ServiceSearchText.Text))
                                .ToList();
                        }
                        else
                        {
                            ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                                .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7).Where(x => x.StatusServicesInOrder.Status
                                .StartsWith(ServiceSearchText.Text))
                                .ToList();
                        }  
                        break;
                }

            }
        }

        private void ServiceSearchBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceSearchBy.SelectedItem != null)
            {
                serviceSearchBy = ServiceSearchBy.SelectedItem.ToString();
            }
        }

        int selectedIdTypeAnalyzer; 
        private void ListAnalyzer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListAnalyzer.SelectedItem != null)
            {
                switch (ListAnalyzer.SelectedItem.ToString())
                {
                    case "Biorad":
                        selectedIdTypeAnalyzer = 1;
                        if (!medLabEntities.OrderAndServices
                            .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                            .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7).ToList().Any())
                        {
                            MessageBox.Show("На данный анализатор доступных анализов нет", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                                .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                                .ToList();
                        break;
                    case "Ledetect":
                        selectedIdTypeAnalyzer = 2;
                        if (!medLabEntities.OrderAndServices
                            .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                            .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                            .ToList().Any())
                        {
                            MessageBox.Show("На данный анализатор доступных анализов нет", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                               .Where(x => x.Service.AnalyzerType == selectedIdTypeAnalyzer || x.Service.AnalyzerType == 3)
                               .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                               .ToList();
                        break;
                    case "Biorad/Ledetect":
                        selectedIdTypeAnalyzer = 3;
                        if (!medLabEntities.OrderAndServices
                            .Where(x => x.Service.AnalyzerType == 3)
                            .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                            .ToList().Any())
                        {
                            MessageBox.Show("На данный анализатор доступных анализов нет", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        ListOutstandingService.ItemsSource = medLabEntities.OrderAndServices
                                .Where(x => x.Service.AnalyzerType == 3)
                                .Where(x => x.IdStatus == 1 || x.IdStatus == 2 || x.IdStatus == 7)
                                .ToList();
                        break;
                    default:
                        MessageBox.Show("Ошибка!");
                        break;
                }
            }
        }
        
        /// <summary>
        /// Запускает анимацию загрузки с прогресс-баром на указанное время
        /// </summary>
        /// <remarks>
        /// Метод выполняет следующие действия:
        /// 1. Блокирует элемент управления ListOutstandingService на время выполнения
        /// 2. Плавно заполняет прогресс-бар (pbStatus) за 5 секунд
        /// 3. По завершении разблокирует ListOutstandingService и показывает уведомление
        /// </remarks>
        private async Task StartLoader() 
        {
            ListOutstandingService.IsEnabled = false;
            int durationSeconds = 5;
            int steps = 100;
            int delay = (durationSeconds * 1000) / steps;

            for (int i = 0; i <= steps; i++)
            {
                pbStatus.Value = i;
                await Task.Delay(delay);
            }
            ListOutstandingService.IsEnabled = true;
            MessageBox.Show("Процесс завершен!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private DateTime dateTimeStartAnalyzer;

        /// <summary>
        /// Отправляет данные анализа в лабораторное оборудование и обрабатывает результат
        /// </summary>
        /// <remarks>
        /// Что делает метод:
        /// 1. Отправляет выбранный анализ на оборудование (Biorad или Ledetect)
        /// 2. Показывает анимацию загрузки
        /// 3. Получает результаты обратно
        /// 4. Проверяет, не отклоняются ли результаты от нормы
        /// 5. Сохраняет результаты в базу данных
        /// 
        /// Особенности:
        /// - Автоматически исправляет формат чисел (заменяет точки на запятые)
        /// - Предупреждает, если результат сильно отличается от нормы
        /// - Работает с двумя типами оборудования
        /// </remarks>
        private async void Button_Click(object sender, EventArgs e)
        {
            dateTimeStartAnalyzer = DateTime.Now;
            if (ListOutstandingService != null)
            {
                try
                {
                    var selectedService = ListOutstandingService.SelectedItem as OrderAndService;
                    if (selectedService != null)
                    {
                        Services services1 = new Services();
                        services1.serviceCode = selectedService.ServiceCode.Value;

                        List<Services> services = new List<Services>()
                        {
                            services1
                        };

                        string patient = selectedService.Order.IdUser.ToString();
                        string name;

                        if (selectedIdTypeAnalyzer == 1)
                        {
                            name = "Biorad";
                        }
                        else
                        {
                            name = "Ledetect";
                        }

                        var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) 
                        {
                            string json = new JavaScriptSerializer().Serialize(new
                            {
                                patient,
                                services
                            });
                            streamWriter.Write(json);
                        }
                        HttpWebResponse httpResponse;
                        try
                        {
                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Ошибка! Перезапустите анализатор.", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            await StartLoader();
                            MessageBox.Show("Услуги успешно отправлены!");
                        }
                        else
                        {
                            MessageBox.Show("Ошибка отправки!");
                            return;
                        }

                        //Получение
                        GetAnalizator getAnalizator = new GetAnalizator();
                        httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";
                        try
                        {
                            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (Stream stream = httpResponse.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(stream);
                                    string json = reader.ReadToEnd();
                                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                                    getAnalizator = serializer.Deserialize<GetAnalizator>(json);
                                    if (getAnalizator.patient == null)
                                    {
                                        MessageBox.Show("Результаты не отправлены!.\n Возможно, пк сильно нагружен.", "Анализатор", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                }
                                var resultService = getAnalizator.Services.First();
                                var StrInitilAVG = medLabEntities.Services.Where(x => x.Code == resultService.serviceCode).FirstOrDefault().InitialAVGDeviation;
                                var StrFinalAVG = medLabEntities.Services.Where(x => x.Code == resultService.serviceCode).FirstOrDefault().FinalAVGDeviation;
                                double InitilAVG = 0;
                                double FinalAVG = 0;
                                if (double.TryParse(StrInitilAVG, out InitilAVG) || double.TryParse(StrFinalAVG, out FinalAVG))
                                {
                                    string originalResult = resultService.result;
                                    string normalizedResult = originalResult.Replace('.', ',');
                                    if (normalizedResult.Contains(','))
                                    {
                                        int decimalIndex = normalizedResult.IndexOf(',');
                                        if (normalizedResult.Length > decimalIndex + 7)
                                        {
                                            normalizedResult = normalizedResult.Substring(0, decimalIndex + 7);
                                        }
                                    }
                                    if (double.TryParse(normalizedResult, out double result))
                                    {
                                        if (result > (FinalAVG * 5) || result < (InitilAVG / 5))
                                        {
                                            var resultQu = MessageBox.Show($"Значения, полученные с анализатора, отклоняются от среднего в 5 раз." +
                                                $"\nВозможен сбой исследователя или некачественный биоматериал", "Анализатор", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                            switch (resultQu)
                                            {
                                                case MessageBoxResult.Yes:
                                                    HelperForSaveResult(3, selectedService, resultService);
                                                    break;
                                                case MessageBoxResult.No:
                                                    HelperForSaveResult(7, selectedService, resultService);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            HelperForSaveResult(3, selectedService, resultService);
                                        }
                                    }
                                    else
                                    {
                                        HelperForSaveResult(3, selectedService, resultService);
                                    }
                                }
                                else
                                {
                                    HelperForSaveResult(3, selectedService, resultService);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                }
                catch (InvalidCastException)
                {
                    MessageBox.Show("Ошибка кода!");
                    return;
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

              
            }
            
        }

        /// <summary>
        /// Сохраняет результаты анализа и обновляет статусы услуг/заказов
        /// </summary>
        /// <param name="status">Новый статус услуги:
        /// <list type="bullet">
        ///   <item><description>1 - Ожидает</description></item>
        ///   <item><description>2 - Отправлена на исследование</description></item>
        ///   <item><description>3 - Выполнена</description></item>
        ///   <item><description>3 - Необходим повторный забор биоматериала</description></item>
        /// </list>
        /// </param>
        /// <param name="selectedService">Выбранная услуга в заказе</param>
        /// <param name="resultService">Объект с результатами анализа</param>
        /// <remarks>
        /// Метод выполняет последовательно:
        /// <list type="number">
        ///   <item><description>Обновляет статус и результат выбранной услуги</description></item>
        ///   <item><description>Создает запись в журнале анализаторов</description></item>
        ///   <item><description>Проверяет завершенность всех услуг в заказе</description></item>
        ///   <item><description>При полном завершении обновляет статус всего заказа</description></item>
        ///   <item><description>Сбрасывает источник данных ListOutstandingService</description></item>
        /// </list>
        public void HelperForSaveResult(int status, OrderAndService selectedService, Services resultService) 
        {
            selectedService.IdStatus = status;
            selectedService.Result = resultService.result;
            medLabEntities.OrderAndServices.AddOrUpdate(selectedService);
            medLabEntities.SaveChanges();

            int idAnalyzer;
            if (selectedIdTypeAnalyzer == 3 || selectedIdTypeAnalyzer == 2)
            {
                idAnalyzer = 2;
            }
            else
            {
                idAnalyzer = 1;
            }
            TimeSpan leadTime = DateTime.Now - dateTimeStartAnalyzer;
            AnalyzerData newAnalyzerData = new AnalyzerData()
            {
                IdAnalyzer = idAnalyzer,
                IdLaboratoryAssistant = _users.Id,
                DateTimeOfOrderReceipt = dateTimeStartAnalyzer,
                IdOrderAndServices = selectedService.Id,
                LeadTime = leadTime
            };
            medLabEntities.AnalyzerDatas.Add(newAnalyzerData);
            medLabEntities.SaveChanges();

            var listServicesInOrder = medLabEntities.OrderAndServices
                .Where(x => x.IdOrder == selectedService.IdOrder)
                .ToList();

            bool allServicesCompleted = listServicesInOrder.All(x => x.IdStatus == 3);
            if (allServicesCompleted)
            {
                MessageBox.Show("Все услуги в заказе выполнены!", "Уведомление",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                var order = medLabEntities.Orders.Where(x => x.Id == selectedService.IdOrder).FirstOrDefault();
                if (order != null)
                {
                    order.IdStatusOrder = 1;
                    medLabEntities.Orders.AddOrUpdate(order);
                    medLabEntities.SaveChanges();
                }
            }
            ListOutstandingService.ItemsSource = null;
        }

    }
}
