using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace form
{
    public partial class Form1 : Form
    {     
        MySqlConnection conn = new MySqlConnection();
        public string response;

        public Form1()
        {
            InitializeComponent();
            clsXMLSettings xml = new clsXMLSettings();
            xmlSettings settings = xml.ReadSettings() ;
            clsWeb web = new clsWeb(settings);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
           clsWeb.sendDataToSite();
            txtview.Text = clsWeb.responseFromServer;
        }
    }
}
