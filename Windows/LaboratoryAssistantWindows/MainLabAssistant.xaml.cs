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
using System.Windows.Shapes;
using System.Timers;
using System.Data.Entity;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using Microsoft.Win32;
using iText.Layout;
using Paragraph = iText.Layout.Element.Paragraph;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Element;
using iText.Layout.Properties;
using Table = iText.Layout.Element.Table;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using Aspose.BarCode.Generation;

namespace MedLab.Windows.LaboratoryAssistantWindows
{
    public partial class MainLabAssistant : Window
    {
        MainWindow _mainWindow;

        public MainLabAssistant(User User, History newHistory, MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            if (User != null)
            {
                StartTimer();
                ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1).ToList();
                _newHistory = newHistory;
                inputUser = User;
                NameRole.Text = "Лаборант";
                FIOUser.Text = inputUser.Surname + " " + inputUser.Name + " " + inputUser.Patronymic;
                List<string> userSearchBy = new List<string> 
                {
                    "Фамилия",
                    "Имя",
                    "Отчество",
                    "Полис",
                    "Номер паспорта",
                    "Серия паспорта"
                };
                foreach (var item in userSearchBy)
                {
                    UserSearchBy.Items.Add(item);
                }

                List<string> orderSearchBy = new List<string>
                {
                    "Статус",
                    "Дата",
                    "Цена"
                };
                foreach (var item in orderSearchBy)
                {
                    OrderSearchBy.Items.Add(item);
                }

                List<string> serviceSearchBy = new List<string>
                {
                    "Номер",
                    "Название",
                    "Статус",
                    "Результат"
                };
                foreach (var item in serviceSearchBy)
                {
                    ServiceSearchBy.Items.Add(item);
                }
            }
            else
            {
                MessageBox.Show($"Ошибка");
                _mainWindow.Show();
                this.Close();
            }
        }

        MedLabEntities medLabEntities = new MedLabEntities();
        private bool checkClosed = false;
        User inputUser = new User();
        History _newHistory = new History();
        private int remainingSeconds;
        private Timer timer;

        /// <summary>
        /// Запускает обратный отсчет таймера с начальным значением 9000 секунд (2,5 часа)
        /// </summary>
        /// <remarks>
        /// Таймер обновляет отображение оставшегося времени каждую секунду.
        /// При достижении 5 минут оставшегося времени:
        /// - Текст таймера меняет цвет на красный
        /// - Выводится предупреждающее сообщение
        /// По истечении времени:
        /// - Останавливает таймер
        /// - Выводит сообщение "Время вышло!"
        /// - Выполняет запись в историю (NewHistory)
        /// - Возвращает на главное окно приложения
        /// </remarks>
        /// <example>
        /// <code>
        /// StartTimer(); // Запускает 2.5-часовой таймер
        /// </code>
        /// </example>
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
                        checkClosed = true;
                        NewHistory(2);
                        _mainWindow.Show();
                        this.Close();
                    });
                }
            };
            timer.Start();
        }


        /// <summary>
        /// Сохраняет запись в историю работы пользователя с указанием причины завершения сеанса
        /// </summary>
        /// <param name="reason">Идентификатор причины завершения:
        /// <list type="bullet">
        ///   <item><description>1 - Обычный выход</description></item>
        ///   <item><description>2 - Автоматическое завершение по таймауту</description></item>
        ///   <item><description>3 - Ошибка по вводу данных</description></item>
        ///   <item><description>3 - Ошибка</description></item>
        /// </list>
        /// </param>
        /// <remarks>
        /// Метод обновляет текущую запись истории:
        /// - Устанавливает дату/время завершения (DateTime.Now)
        /// - Задает указанную причину завершения
        /// - Сохраняет изменения в базе данных (medLabEntities)
        /// </remarks>
        /// <example>
        /// <code>
        /// // Завершение сеанса по таймауту
        /// NewHistory(2);
        /// 
        /// // Обычный выход пользователя
        /// NewHistory(1);
        /// </code>
        /// </example>
        /// <seealso cref="MedLabEntities"/>
        public void NewHistory(int reason) 
        {
            _newHistory.DateEnd = DateTime.Now;
            _newHistory.IdReason = reason;
            medLabEntities.Histories.Add(_newHistory);
            medLabEntities.SaveChanges();
        }
        private void ExitMainForLabAssitant_Click(object sender, RoutedEventArgs e)
        {
            checkClosed = true;
            NewHistory(1);
            _mainWindow.Show();
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!checkClosed)
            {
                NewHistory(1);
            }
            _mainWindow.Show();
        }

        private string userSearchBy;

        private void SearchText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userSearchBy))
            {
                ListUsers.ItemsSource = null;
                switch (userSearchBy)
                {
                    case "Фамилия":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.Surname.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    case "Имя":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.Name.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    case "Отчество":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.Patronymic.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    case "Полис":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.InsurancePolicyNumber.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    case "Номер паспорта":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.PassportNumber.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    case "Серия паспорта":
                        ListUsers.ItemsSource = medLabEntities.Users.Where(u => u.IdUserType == 1 && u.PassportSeries.StartsWith(UserSearchText.Text)).ToList();
                        break;
                    default:
                        break;
                }
                
            }
        }

        private void SearchBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserSearchBy.SelectedItem != null)
            {
                userSearchBy = UserSearchBy.SelectedItem.ToString();
            }
        }
        private User selectedUser;
        private void ListUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListUsers.SelectedItem != null)
            {
                selectedUser = ListUsers.SelectedItem as User;
                ListOrdersForUser.ItemsSource = medLabEntities.Orders.Where(x => x.IdUser == selectedUser.Id).ToList();
            }
        }
        private Order selectedOrder;
        private void ListOrdersForUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOrdersForUser.SelectedItem != null)
            {
                selectedOrder = ListOrdersForUser.SelectedItem as Order;
                ListSrvicesInOrder.ItemsSource = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id).ToList();
            }
        }

        private void ChangeOrdersClick(object sender, RoutedEventArgs e)
        {
            if (selectedOrder == null)
            {
                MessageBox.Show("Перед изменением заказа выберите услугу");
                return;
            }
            ChangeOrderByAssistant changeOrderByAssistant = new ChangeOrderByAssistant(selectedOrder);
            changeOrderByAssistant.Show();
        }

        private string orderSearchBy;
        private void OrderSearchBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrderSearchBy.SelectedItem != null)
            {
                orderSearchBy = OrderSearchBy.SelectedItem.ToString();
                if (orderSearchBy == "Дата")
                {
                    OrderSearchText.Visibility = Visibility.Collapsed;
                    DateGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    OrderSearchText.Visibility = Visibility.Visible;
                    DateGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(orderSearchBy))
                {
                    ListOrdersForUser.ItemsSource = null;
                    switch (orderSearchBy)
                    {
                        case "Дата":
                            DateTime? startDate = null;
                            DateTime? endDate = null;

                            if (!string.IsNullOrWhiteSpace(StartDate.Text))
                                startDate = DateTime.Parse(StartDate.Text);

                            if (!string.IsNullOrWhiteSpace(EndDate.Text))
                                endDate = DateTime.Parse(EndDate.Text);

                            IQueryable<Order> query = medLabEntities.Orders.Where(x => x.IdUser == selectedUser.Id);

                            if (startDate.HasValue && endDate.HasValue)
                            {
                                query = query.Where(x => x.DateOfCreation >= startDate.Value && x.DateOfCreation <= endDate.Value);
                            }
                            else if (startDate.HasValue)
                            {
                                query = query.Where(x => x.DateOfCreation >= startDate.Value);
                            }
                            else if (endDate.HasValue)
                            {
                                query = query.Where(x => x.DateOfCreation <= endDate.Value);
                            }

                            ListOrdersForUser.ItemsSource = query.ToList();
                            break;
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат даты. Пожалуйста, проверьте ввод.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderSearchText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(orderSearchBy))
            {
                try
                {
                    ListOrdersForUser.ItemsSource = null;
                    switch (orderSearchBy)
                    {
                        case "Статус":
                            ListOrdersForUser.ItemsSource = medLabEntities.Orders
                                .Where(x => x.IdUser == selectedUser.Id
                                    && x.StatusOrder.Status.StartsWith(OrderSearchText.Text))
                                .ToList();
                            break;
                        case "Цена":
                            var searchByPrice = medLabEntities.Orders
                                .Where(x => x.IdUser == selectedUser.Id)
                                .ToList();
                            ListOrdersForUser.ItemsSource = searchByPrice
                                .Where(x => x.FullPrice.ToString().StartsWith(OrderSearchText.Text))
                                .ToList();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        private void AddNewOrdersClick(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                AddNewOrderWindow addNewOrderWindow = new AddNewOrderWindow(selectedUser);
                addNewOrderWindow.Show();
            }
        }

        private string serviceSearchBy;
        private void ServiceSearchBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceSearchBy.SelectedItem != null)
            {
                serviceSearchBy = ServiceSearchBy.SelectedItem.ToString();
            }
        }

        private void ServiceSearchText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(serviceSearchBy))
            {
                ListSrvicesInOrder.ItemsSource = null;
                switch (serviceSearchBy)
                {
                    case "Номер":
                        ListSrvicesInOrder.ItemsSource = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id && x.IdOrder.ToString().StartsWith(ServiceSearchText.Text)).ToList();
                        break;
                    case "Название":
                        ListSrvicesInOrder.ItemsSource = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id && x.Service.ServiceName.StartsWith(ServiceSearchText.Text)).ToList();
                        break;
                    case "Статус":
                        ListSrvicesInOrder.ItemsSource = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id && x.StatusServicesInOrder.Status.StartsWith(ServiceSearchText.Text)).ToList();
                        break;
                    case "Результат":
                        ListSrvicesInOrder.ItemsSource = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id && x.Result.StartsWith(ServiceSearchText.Text)).ToList();
                        break;
                }

            }
        }


        /// <summary>
        /// Генерирует PDF-отчет с результатами анализов для выбранного заказа
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        /// <remarks>
        /// Метод выполняет следующие действия:
        /// <list type="number">
        ///   <item><description>Открывает диалог сохранения файла с фильтром .pdf</description></item>
        ///   <item><description>Создает PDF-документ с фирменным стилем лаборатории</description></item>
        ///   <item><description>Добавляет информацию о пациенте и заказе</description></item>
        ///   <item><description>Формирует таблицу с результатами анализов</description></item>
        ///   <item><description>Добавляет итоговую сумму и юридическое уведомление</description></item>
        ///   <item><description>Включает QR-коды и информацию о датах в нижний колонтитул</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// Пример вызова:
        /// <code>
        /// private void Button_Click(object sender, RoutedEventArgs e)
        /// {
        ///     GeneratePDF_Click(sender, e);
        /// }
        /// </code>
        private void GeneratePDF_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrder != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files|*.pdf";
                saveFileDialog.Title = "Сохранить PDF файл";
                saveFileDialog.FileName = "Service.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (PdfWriter writer = new PdfWriter(filePath))
                    using (PdfDocument pdf = new PdfDocument(writer))
                    using (Document document = new Document(pdf, PageSize.A4))
                    {
                        try
                        {
                            document.SetMargins(30, 30, 30, 30);
                            PdfFont font = PdfFontFactory.CreateFont("Fonts/TIMES.TTF", PdfEncodings.IDENTITY_H);
                            PdfFont boldFont = PdfFontFactory.CreateFont("Fonts/TIMESBD.TTF", PdfEncodings.IDENTITY_H);
                            PdfFont italicFont = PdfFontFactory.CreateFont("Fonts/TIMESI.TTF", PdfEncodings.IDENTITY_H);
                            ImageData imageData = ImageDataFactory.Create("Image/Logo.jpg");
                            iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER).SetWidth(170).SetHeight(50);
                            document.Add(image);
                            Paragraph header = new Paragraph("ООО МедЛабЭкспресс").SetFontSize(14).SetFont(boldFont)
                                .SetTextAlignment(TextAlignment.CENTER);
                            document.Add(header);
                            Paragraph header2 = new Paragraph("Клинико-диагностическая лаборатория").SetFontSize(12).SetFont(boldFont)
                                .SetTextAlignment(TextAlignment.CENTER);
                            document.Add(header2);
                            // Первый параграф с контактной информацией (уже выровнен по правому краю)
                            Paragraph info = new Paragraph("2281337 Пермский край" +
                                "\nг.Пермь, ул. Сысольская, 14а" +
                                "\nEmail: stafflll@mail.ru\n" +
                                "тел. : +7 (8005) 553-535")
                                .SetFont(font)
                                .SetFontSize(12)
                                .SetTextAlignment(TextAlignment.RIGHT);
                            document.Add(info);
                            Paragraph fioUser = new Paragraph()
                                .Add(new Text("Пациент: ").SetFont(font).SetFontSize(14))
                                .Add(new Text($"{selectedUser.Surname} {selectedUser.Name} {selectedUser.Patronymic}").SetFont(boldFont).SetFontSize(14))
                                .SetTextAlignment(TextAlignment.LEFT);
                            document.Add(fioUser);
                            Paragraph birthUser = new Paragraph().Add(new Text("Дата рождения: ").SetFont(font).SetFontSize(14))
                                .Add(new Text(selectedUser.DateOfBirth.ToString()).SetFont(boldFont).SetFontSize(14));
                            document.Add (birthUser);
                            Paragraph idUser = new Paragraph($"ID клиента {selectedUser.Id}").SetFontSize(12).SetFont(italicFont).SetTextAlignment(TextAlignment.CENTER);
                            document.Add(idUser);
                            LineSeparator line = new LineSeparator(new SolidLine()) 
                                .SetWidth(UnitValue.CreatePercentValue(100)) 
                                .SetMarginTop(10) 
                                .SetMarginBottom(10);  
                            document.Add(line);
                            Paragraph result = new Paragraph("Результаты тестирования").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER);
                            document.Add(result);
                            Table headerTable = new Table(2)
                                .SetWidth(UnitValue.CreatePercentValue(100));   

                            Cell orgCell = new Cell()
                                .Add(new Paragraph("Организация: МедЛаб")).SetFont(font)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(50));  

                            Cell regNumCell = new Cell()
                                .Add(new Paragraph($"Регистрационный номер: {selectedOrder.Id}")).SetFont(font)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetWidth(UnitValue.CreatePercentValue(50));

                            headerTable.AddCell(orgCell);
                            headerTable.AddCell(regNumCell);
                            document.Add(headerTable);

                            Table infoService = new Table(5).SetWidth(UnitValue.CreatePercentValue(100));
                            infoService.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

                            Cell nameService = new Cell() 
                                .Add(new Paragraph("Название").SetFont(font).SetTextAlignment(TextAlignment.CENTER))
                                .SetWidth(UnitValue.CreatePercentValue(35));

                            Cell resultService = new Cell() 
                                .Add(new Paragraph("Результат").SetFont(font).SetTextAlignment(TextAlignment.CENTER))
                                .SetWidth(UnitValue.CreatePercentValue(20));

                            Cell normService = new Cell() 
                                .Add(new Paragraph("Норма").SetFont(font).SetTextAlignment(TextAlignment.CENTER))
                                .SetWidth(UnitValue.CreatePercentValue(25));

                            Cell costService = new Cell()
                                .Add(new Paragraph("Цена").SetFont(font).SetTextAlignment(TextAlignment.CENTER))
                                .SetWidth(UnitValue.CreatePercentValue(25));

                            Cell analyzerService = new Cell() 
                                .Add(new Paragraph("Анализатор").SetFont(font).SetTextAlignment(TextAlignment.CENTER))
                                .SetWidth(UnitValue.CreatePercentValue(20));

                            infoService.AddCell(nameService);
                            infoService.AddCell(resultService);
                            infoService.AddCell(normService);
                            infoService.AddCell(costService);
                            infoService.AddCell(analyzerService);

                            var listService = medLabEntities.OrderAndServices.Where(x => x.IdOrder == selectedOrder.Id).ToList();
                            decimal summa = 0;
                            try
                            {
                                foreach (var item in listService)
                                {
                                    summa += item.Service.Price;
                                    infoService.AddCell(new Cell().Add(new Paragraph(item.Service.ServiceName)
                                        .SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                                    infoService.AddCell(new Cell().Add(new Paragraph(item.Result)
                                        .SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                                    infoService.AddCell(new Cell().Add(new Paragraph(item.Service.InitialAVGDeviation + "-" + item.Service.FinalAVGDeviation)
                                        .SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                                    infoService.AddCell(new Cell().Add(new Paragraph(item.Service.Price.ToString())
                                        .SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                                    infoService.AddCell(new Cell().Add(new Paragraph(item.AnalyzerDatas
                                        .Where(x => x.IdOrderAndServices == item.Id).FirstOrDefault().Analyzer.AnalyzerName)
                                        .SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                                }
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Ошибка! Работы связанные с анализатором не закончены!");
                                return;
                            }

                            document.Add(infoService);
                            Paragraph fullSumma = new Paragraph().Add(new Text("Итого: ").SetFont(font).SetFontSize(14))
                                .Add(new Text(summa.ToString() + "руб.").SetFont(boldFont).SetFontSize(14))
                                .SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(10);
                            document.Add(fullSumma);

                            Div footerDiv = new Div()
                            .SetFixedPosition(
                                document.GetLeftMargin(), 
                                30,                      
                                document.GetPageEffectiveArea(PageSize.A4).GetWidth() 
                            )
                            .SetMarginTop(20);

                            Paragraph bottom = new Paragraph("Обращаем Ваше внимание на то, что интерпретация результатов исследований," +
                                " установление диагноза, а также назначение лечения, в соответствии с Федеральным законом № 323-ФЗ" +
                                " \"Об основах охраны здоровья граждан в Российской Федерации от 21 ноября 2011 года," +
                                " должны производиться врачом соответствующей специализации\"")
                                .SetFont(boldFont)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(8)
                                .SetMarginBottom(10);

                            footerDiv.Add(bottom);

                            // Таблица с QR-кодами внизу
                            Table bottomTable = new Table(4)
                                .SetWidth(UnitValue.CreatePercentValue(100));

                            ImageData aQRPath = ImageDataFactory.Create("Image/AntonQR.png");
                            iText.Layout.Element.Image aQR = new iText.Layout.Element.Image(aQRPath)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER).SetWidth(60).SetHeight(60);
                            Cell antonQr = new Cell().Add(aQR).SetWidth(UnitValue.CreatePercentValue(20)).SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            ImageData kQRPath = ImageDataFactory.Create("Image/KrisQR.png");
                            iText.Layout.Element.Image kQR = new iText.Layout.Element.Image(kQRPath)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER).SetWidth(60).SetHeight(60);
                            Cell krisQr = new Cell().Add(kQR).SetWidth(UnitValue.CreatePercentValue(20)).SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            ImageData printPath = ImageDataFactory.Create("Image/Pech.jpg");
                            iText.Layout.Element.Image printImage = new iText.Layout.Element.Image(printPath)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER).SetWidth(60).SetHeight(60);
                            Cell img = new Cell().Add(printImage).SetWidth(UnitValue.CreatePercentValue(20)).SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            Cell serviceInfo = new Cell().Add(new Paragraph()
                                .Add(new Text("Дата забора биоматериала: ").SetFont(italicFont).SetFontSize(10))
                                .Add(new Text(selectedOrder.DateOfCreation.ToString()).SetFont(boldFont).SetFontSize(10))
                                .Add(new Text("\nДата доставки биоматериала в лабораторию: ").SetFont(italicFont).SetFontSize(10))
                                .Add(new Text(selectedOrder.DateOfCreation.ToString()).SetFont(boldFont).SetFontSize(10))
                                .Add(new Text("\nДата готовности: ").SetFont(italicFont).SetFontSize(10))
                                .Add(new Text(selectedOrder.DateOfCreation.ToString()).SetFont(boldFont).SetFontSize(10)))
                                .SetWidth(UnitValue.CreatePercentValue(40)).SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            bottomTable.AddCell(antonQr);
                            bottomTable.AddCell(krisQr);
                            bottomTable.AddCell(img);
                            bottomTable.AddCell(serviceInfo);

                            footerDiv.Add(bottomTable);
                            document.Add(footerDiv);

                            document.Close();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                document.Close();
                            }
                            catch (Exception ex1)
                            {
                                MessageBox.Show(ex1.Message);
                                return;
                            }

                            MessageBox.Show(ex.Message);
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Сохранение отменено.");
                }
            }
        }

        private void CrateNewUser_Click(object sender, RoutedEventArgs e)
        {
            AddNewUserByAssistant addNewUserByAssistant = new AddNewUserByAssistant();
            addNewUserByAssistant.Show();
        }

        private void GenerateBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (ListSrvicesInOrder.SelectedItem != null)
            {
                try
                {
                    var selectedService = ListSrvicesInOrder.SelectedItem as OrderAndService;
                    string imageType = "jpeg";
                    string imagePath = $"{Environment.CurrentDirectory}\\Barcode.{imageType}";
                    if (selectedService.BarcodeCode != null)
                    {
                        using (var generator = new BarcodeGenerator(EncodeTypes.Code128, selectedService.BarcodeCode))
                        {
                            generator.Parameters.Barcode.XDimension.Pixels = 5;
                            generator.Save(imagePath, BarCodeImageFormat.Jpeg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files|*.pdf";
                saveFileDialog.Title = "Сохранить PDF файл";
                saveFileDialog.FileName = "PDFBarcode.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (PdfWriter writer = new PdfWriter(filePath))
                    using (PdfDocument pdf = new PdfDocument(writer))
                    using (Document document = new Document(pdf))
                    {
                        try
                        {
                            PdfFont font = PdfFontFactory.CreateFont("Fonts/TIMES.TTF", PdfEncodings.IDENTITY_H);
                            ImageData imageData = ImageDataFactory.Create($"{Environment.CurrentDirectory}\\Barcode.jpeg");
                            iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData)
                                .SetWidth(200).SetHeight(70);
                            document.Add(image);
                            document.Close();
                        }
                        catch (Exception ex)
                        {
                            document.Close();
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Сохранение отменено.");
                }
            }
        }
    }
}
