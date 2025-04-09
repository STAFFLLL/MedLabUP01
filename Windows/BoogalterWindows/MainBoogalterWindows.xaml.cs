using iText.Layout.Element;
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

namespace MedLab.Windows.BoogalterWindows
{
    public partial class MainBoogalterWindows : Window
    {
        private MainWindow _mainWindow;
        private User _users;
        private History _newHistory;
        private bool checkClosed = false;
        private MedLabEntities medLabEntities = new MedLabEntities();

        public MainBoogalterWindows(MainWindow main_, User User, History History)
        {
            InitializeComponent();
            try
            {
                _mainWindow = main_;
                _users = User;
                _newHistory = History;
                ListAccount.ItemsSource = medLabEntities.Orders.Where(x => x.IdStatusOrder == 1).ToList();
                InsuranceCompanyCB.ItemsSource = medLabEntities.InsuranceCompanies.Select(x => x.Name).ToList();
                FIOUser.Text = $"{_users.Surname} {_users.Name} {_users.Patronymic}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NewHistory(int reason)
        {
            _newHistory.DateEnd = DateTime.Now;
            _newHistory.IdReason = reason;
            medLabEntities.Histories.Add(_newHistory);
            medLabEntities.SaveChanges();
        }

        private void ExitMainForBuh_Click(object sender, RoutedEventArgs e)
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

        private DateTime? dateStart = null;
        private DateTime? dateEnd = null;
        private int companyId = 0;
        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(StartDate.Text))
            {
                if (DateTime.TryParse(StartDate.Text, out var parsedStartDate))
                {
                    dateStart = parsedStartDate;
                }
                else
                {
                    MessageBox.Show("Некорректный формат начальной даты");
                    dateStart = null;
                }
            }
            else
            {
                dateStart = null;
            }
            ApplyFilters();
        }

        private void EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EndDate.Text))
            {
                if (DateTime.TryParse(EndDate.Text, out var parsedEndDate))
                {
                    dateEnd = parsedEndDate;
                }
                else
                {
                    MessageBox.Show("Некорректный формат конечной даты");
                    dateEnd = null;
                }
            }
            else
            {
                dateEnd = null;
            }
            ApplyFilters();
        }

        private void InsuranceCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InsuranceCompanyCB.SelectedItem != null)
            {
                string nameCompany = InsuranceCompanyCB.SelectedItem.ToString();
                var company = medLabEntities.InsuranceCompanies.FirstOrDefault(x => x.Name == nameCompany);
                if (company != null)
                {
                    companyId = company.Id;
                }
                else
                {
                    companyId = 0;
                }
            }
            else
            {
                companyId = 0;
            }
            ApplyFilters();
        }

        private IQueryable<Order> query;
        /// <summary>
        /// Применяет фильтры к списку заказов и обновляет отображение
        /// </summary>
        /// <remarks>
        /// Доступные фильтры:
        /// - По страховой компании (если companyId > 0)
        /// - По дате создания (диапазон или отдельные даты)
        /// 
        /// Обновляет ListAccount.ItemsSource с отфильтрованными данными
        /// </remarks>
        private void ApplyFilters()
        {
            query = medLabEntities.Orders.Where(x => x.IdStatusOrder == 1);

            if (companyId > 0)
            {
                query = query.Where(x => x.User.InsuranceCompany.Id == companyId);
            }
           
            if (dateStart.HasValue && dateEnd.HasValue)
            {
                query = query.Where(d => d.DateOfCreation >= dateStart.Value && d.DateOfCreation <= dateEnd.Value);
            }

            else if (dateStart.HasValue)
            {
                query = query.Where(d => d.DateOfCreation >= dateStart.Value);
            }

            else if (dateEnd.HasValue)
            {
                query = query.Where(d => d.DateOfCreation <= dateEnd.Value);
            }

            ListAccount.ItemsSource = query.ToList();
        }

        /// <summary>
        /// Генерирует уникальный номер счёта для страховой компании
        /// </summary>
        /// <returns>
        /// Строка с номером счёта в формате: ACC-ГГГГ-XXXXX
        /// где ГГГГ - текущий год, XXXXX - порядковый номер
        /// </returns>
        /// <example>
        /// Пример результата: "ACC-2023-10025"
        /// </example>
        /// <remarks>
        /// Номер формируется на основе:
        /// - Текущего года (DateTime.Now.Year)
        /// - Последнего ID в таблице AccountForInsuranceCompanies + 1
        /// </remarks>
        public string FormingInvoiceNumber() 
        {
            string year = DateTime.Now.Year.ToString();
            int lastSerialNumber = medLabEntities.AccountForInsuranceCompanies.OrderByDescending(x => x.Id).FirstOrDefault().Id;
            return $"ACC-{year}-{++lastSerialNumber}";
        }

        private void AddNewAccount_Click(object sender, RoutedEventArgs e)
        {
            if (companyId != 0 && dateEnd.HasValue && dateStart.HasValue)
            {
                decimal amount = 0;
                foreach (var item in query)
                {
                    amount += item.FullPrice;
                }
                AccountForInsuranceCompany accountForInsuranceCompanies_ = new AccountForInsuranceCompany()
                {
                    IdUser = _users.Id,
                    InvoiceNumber = FormingInvoiceNumber(),
                    InvoiceDate = dateStart,
                    Amount = amount,
                    IdInsuranceCompany = companyId,
                    IdStatusAccount = 2,
                    EndDate = dateEnd,
                };
                medLabEntities.AccountForInsuranceCompanies.Add(accountForInsuranceCompanies_);
                medLabEntities.SaveChanges();
                MessageBox.Show("Успешно", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Страховая компания или временной промежуток не выбраны!", "MedLabApp", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
        }
    }
}
