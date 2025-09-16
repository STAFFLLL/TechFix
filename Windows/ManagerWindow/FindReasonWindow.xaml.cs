using Notification.Wpf;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
namespace TechFix.Windows.ManagerWindow
{
    public partial class FindReasonWindow : Window
    {
        public FindReasonWindow(Application application)
        {
            InitializeComponent();
            _application = application;
        }
        private Application _application;
        private NotificationManager notification = new NotificationManager();
        private void SaveReasonApplication_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new TechFixDBEntities()) 
            {
                _application.IdApplicationStatus = 7;
                var masApp = db.MasterInApplication.Where(m => m.IdApplication == _application.Id).FirstOrDefault();
                string oldCoommnets = masApp.Comments;
                masApp.Comments = oldCoommnets + "\n" + ReasonTextBox.Text;
                db.Application.AddOrUpdate(_application);
                db.MasterInApplication.AddOrUpdate(masApp);
                db.SaveChanges();
                notification.Show("Успешно", NotificationType.Success);
                this.Close();
            }
        }
    }
}
