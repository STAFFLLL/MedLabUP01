using MedLab.Windows.HelpWindows;
using MedLab.Windows.LaboratoryAssistantWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mail_LIB;
using MedLab.Windows.AdministratorWindows;
using MedLab.Windows.LaboratoryAssistantResearcherWindows;
using MedLab.Windows.BoogalterWindows;
using MedLab.Windows.UserWindows;

namespace MedLab
{
    public class IncorrectUser
    {
        public IncorrectUser(string userName, int incorrectInput) 
        {
            this.userName = userName;
            this.incorrectInput = incorrectInput;
        }
        public string userName { get; set; }
        public int incorrectInput { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        MedLabEntities medlabEntities = new MedLabEntities();
        private List<IncorrectUser> incorrectUsers = new List<IncorrectUser>(); 
        private void ShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordAvt.Visibility == Visibility.Visible)
            {
                string pw = PasswordAvt.Password;
                PasswordAvt.Visibility = Visibility.Hidden;
                VisiblePassword.Visibility = Visibility.Visible;
                VisiblePassword.Text = pw;
            }
            else
            {
                string pw = VisiblePassword.Text;
                VisiblePassword.Visibility = Visibility.Hidden;
                PasswordAvt.Visibility = Visibility.Visible;
                PasswordAvt.Password = pw;
            }
            if (RegPassword.Visibility == Visibility.Visible)
            {
                string pw = RegPassword.Password;
                RegPassword.Visibility = Visibility.Hidden;
                RegVisiblePassword.Visibility = Visibility.Visible;
                RegVisiblePassword.Text = pw;
            }
            else
            {
                string pw = RegVisiblePassword.Text;
                RegVisiblePassword.Visibility = Visibility.Hidden;
                RegPassword.Visibility = Visibility.Visible;
                RegPassword.Password = pw;
            }
        }

        private void AvtClear_Click(object sender, RoutedEventArgs e)
        {
            string err = Cryptographer.HashPassword(PasswordAvt.Password);
            AvtLogin.Text = string.Empty;
            PasswordAvt.Password = string.Empty;
            VisiblePassword.Text = string.Empty;
        }


        private void RegSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RegSurname.Text) || string.IsNullOrEmpty(RegName.Text) || string.IsNullOrEmpty(RegPatronymic.Text) 
                || string.IsNullOrEmpty(RegDateOfBirth.Text)
                   || string.IsNullOrEmpty(RegPassportSeries.Text) || string.IsNullOrEmpty(RegPassportNumber.Text) || string.IsNullOrEmpty(RegEmail.Text) 
                   || string.IsNullOrEmpty(RegIncurancePolicyNumber.Text) || string.IsNullOrEmpty(RegLogin.Text) || string.IsNullOrEmpty(RegPassword.Password) || string.IsNullOrEmpty(RegNumberTel.Text))
            {
                MessageBox.Show("Вы ввели не все данные!");
            }
            else
            {
                string password = Cryptographer.HashPassword(RegPassword.Password);
                if (LoginHelper.СheckMail(RegEmail.Text) && LoginHelper.Check_login(RegLogin.Text) && LoginHelper.Check_password(RegPassword.Password))
                {
                    User newUser = new User()
                    {
                        Surname = RegSurname.Text,
                        Name = RegName.Text,
                        Patronymic = RegPatronymic.Text,
                        DateOfBirth = Convert.ToDateTime(RegDateOfBirth.Text),
                        PassportSeries = RegPassportSeries.Text,
                        PassportNumber = RegPassportNumber.Text,
                        Email = RegEmail.Text,
                        PhoneNumber = RegNumberTel.Text,
                        IdUserType = 1,
                        InsurancePolicyNumber = RegIncurancePolicyNumber.Text,
                        Login = RegLogin.Text,
                        Password = password
                    };
                    medlabEntities.Users.Add(newUser);
                    medlabEntities.SaveChanges();
                }
            }
        }

        
        private void RegClear_Click(object sender, RoutedEventArgs e)
        {
            RegSurname.Text = string.Empty;
            RegName.Text = string.Empty;
            RegPatronymic.Text = string.Empty;
            RegDateOfBirth.Text = string.Empty;
            RegPassportSeries.Text = string.Empty;
            RegPassportNumber.Text = string.Empty;
            RegEmail.Text = string.Empty;
            RegIncurancePolicyNumber.Text = string.Empty;
            RegPassword.Password = string.Empty;
            RegVisiblePassword.Text = string.Empty;
        }


        public static History newHistory = new History();
        public static User userLogin;
        private bool isCorrect = false;
        MedLabEntities MedLabEntitiesForHistory;

        /// <summary>
        /// Выполняет аутентификацию пользователя и открывает соответствующее окно приложения
        /// в зависимости от типа пользователя (Лаборант, Администратор, Клиент и т.д.)
        /// </summary>
        /// <remarks>
        /// Метод проверяет логин и пароль пользователя, проверяет наличие незавершенных сеансов,
        /// записывает новую запись в историю входов и открывает соответствующее рабочее окно.
        /// Для лаборантов дополнительно проверяется время кварцевания после предыдущего выхода.
        /// </remarks>
        /// <returns>
        /// Фактически возвращаемое значение определяется через поле isCorrect:
        /// true - аутентификация успешна, false - аутентификация не удалась.
        /// </returns>
        public void LoginUser() 
        {
            MedLabEntitiesForHistory = new MedLabEntities();
            if (MedLabEntitiesForHistory.Users.Any(x => x.Login == AvtLogin.Text))
            {
                History lastExitUser = null;
                userLogin = MedLabEntitiesForHistory.Users.FirstOrDefault(x => x.Login == AvtLogin.Text);
                if (Cryptographer.VerifyPassword(PasswordAvt.Password, userLogin.Password))
                {
                    switch (userLogin.UserType.Type)
                    {
                        case "Лаборант":
                        case "Лаборант-исследователь":
                            lastExitUser = userLogin.Histories
                                .Where(x => x.IdUser == userLogin.Id)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();

                            if (lastExitUser != null && lastExitUser.ReasonsForExit.Reason == "Время вышло"
                                && TimeSpan.FromMinutes(15) > DateTime.Now - lastExitUser.DateEnd)
                            {
                                MessageBox.Show("Время кварцевания еще не вышло!");
                                isCorrect = false;  
                                return;
                            }

                            newHistory.IdUser = userLogin.Id;
                            newHistory.DateStart = DateTime.Now;
                            newHistory.Ip = HelperIP.GetIp();

                            if (userLogin.UserType.Type == "Лаборант")
                            {
                                var mainLabAssistant = new MainLabAssistant(userLogin, newHistory, this);
                                mainLabAssistant.Show();
                            }
                            else
                            {
                                var mainLabAssistantRes = new MainLaboratoryAssistantResearcher(this, newHistory, userLogin);
                                mainLabAssistantRes.Show();
                            }

                            this.Hide();
                            isCorrect = true;
                            break;
                        case "Администратор":
                                newHistory.IdUser = userLogin.Id;
                                newHistory.DateStart = DateTime.Now;
                                newHistory.Ip = HelperIP.GetIp();
                                MainAdministranotWindows mainAdministranotWindows = new MainAdministranotWindows(this, newHistory, userLogin);
                                mainAdministranotWindows.Show();
                                this.Hide();
                                isCorrect = true;
                            break;
                        case "Клиент":
                            newHistory.IdUser = userLogin.Id;
                            newHistory.DateStart = DateTime.Now;
                            newHistory.Ip = HelperIP.GetIp();
                            MainUserWindows mainUser = new MainUserWindows(this, userLogin, newHistory);
                            mainUser.Show();
                            this.Hide();
                            isCorrect = true;
                            break;
                        case "Бухгалтер":
                            newHistory.IdUser = userLogin.Id;
                            newHistory.DateStart = DateTime.Now;
                            newHistory.Ip = HelperIP.GetIp();
                            MainBoogalterWindows mainBoogalterWindows = new MainBoogalterWindows(this, userLogin, newHistory);
                            mainBoogalterWindows.Show();
                            this.Hide();
                            isCorrect = true;
                            break;
                        default:
                            MessageBox.Show("Ошибка БД");
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Пароль не верен!");
                    isCorrect = false;
                }
            }
            else
            {
                MessageBox.Show("Логин не найден!");
                isCorrect = false;
            }
        }

        private void AvtEnter_Click(object sender, EventArgs e)
        {   
            if (!string.IsNullOrEmpty(AvtLogin.Text) && !string.IsNullOrEmpty(PasswordAvt.Password))
            {
                LoginUser();
                if (!isCorrect)
                {
                    if (!incorrectUsers.Any(x=> x.userName == AvtLogin.Text))
                    {
                        incorrectUsers.Add(new IncorrectUser(AvtLogin.Text, 0));
                    }
                    var incorrectUser = incorrectUsers.Where(x => x.userName == AvtLogin.Text).FirstOrDefault();
                    if (incorrectUser.incorrectInput > 1)
                    {
                        InputCaptchaWindow inputCaptchaWindow = new InputCaptchaWindow(this, userLogin);
                        inputCaptchaWindow.Show();
                        this.Hide();
                    }
                    int NumberOfIncorrectInputs = incorrectUser.incorrectInput;
                    string userName = incorrectUser.userName;
                    incorrectUsers.Remove(incorrectUser);
                    incorrectUsers.Add(new IncorrectUser(userName, incorrectUser.incorrectInput + 1));
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
        }
    }
}
