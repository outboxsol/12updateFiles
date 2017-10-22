using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace updateFiles
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
           
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DropContractTable();
        }

        //variables
        public string mysqlconnectionstring = "Server=localhost;Database=files;Uid=root;Pwd=mysql;";
        MySqlConnection connection;
        string instrument;
        int token;
        public double count;
        public double exceptioncount;
        void readfile()
        { 

        string sourcepath = "d:\\contract.txt";
        var source = System.IO.File.ReadAllLines(sourcepath);
        
            
        for (int i = 1; i < source.Length; i++)
        {

            var sourcecells = source[i].Split('|');
            string expirydate = gettimefromepochtime(Convert.ToDouble(sourcecells[6])).ToString("ddMMMyyyy").ToUpper();
            //parse futstk

            if ((expirydate == "30NOV2017") || (expirydate == "26OCT2017"))
            {

                if ((sourcecells[53] != "0" || sourcecells[0] != "0") && ((sourcecells[2] == "FUTSTK") || (sourcecells[2] == "FUTIDX")))
                {

                    //Debug.WriteLine(sourcecells[0] + "----" + sourcecells[53]);

                    string instrument = (sourcecells[2] + "-" + sourcecells[3] + "-" + expirydate).ToUpper();
                    insertData(instrument, sourcecells[0]);
                }

                // parsing for OPTSTK
                if ((sourcecells[53] != "0" || sourcecells[0] != "0") && ((sourcecells[2] == "OPTSTK") || (sourcecells[2] == "OPTIDX")))
                {


                    //FORMAT OPTSTK-SYMOL_NAME-DDMMMYYYY-STRIKEPRICE-OPTIONTYPE

                    string strikePrice = Convert.ToString((Convert.ToInt64(sourcecells[7]) / 100).ToString("0.00"));

                    string optionType = sourcecells[8];
                    string instrument = (sourcecells[2] + "-" + sourcecells[3] + "-" + expirydate).ToUpper() + "-" + strikePrice + "-" + optionType;
                    System.Diagnostics.Debug.WriteLine(instrument);
                    insertData(instrument, sourcecells[0]);
                    int X = 0;
                }
    }

            

        }
        MessageBox.Show("Total Count = " + count + "\nException count = " + exceptioncount);

        }


        DateTime  gettimefromepochtime(double timestamp)
        {
           

            // First make a System.DateTime equivalent to the UNIX Epoch.
            System.DateTime dateTime = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);

            // Add the number of seconds in UNIX timestamp to be converted.
            return dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
            
        }

        void insertData(string instrument, string token)
        {

            try
            {
                MySqlConnection connection = new MySqlConnection(mysqlconnectionstring);
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Connection state was : " + connection.State + " hence opened " + "\n" + " count : = " + count + "Exception count = " + exceptioncount);
                }
                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO contract(instrument,token) VALUES (@instrument,@token)";
                 
                command.Prepare();
                //----------------*********************---------------------
                command.Parameters.AddWithValue("@instrument", instrument);
                command.Parameters.AddWithValue("@token", token);
                
                //----------------*********************---------------------
                command.ExecuteNonQuery();
                connection.Close();//have to come up with logic to close it after all the contracts are deleted
                count++;
              
            }
            catch (MySqlException mysql)
            {
                exceptioncount++;
                System.Diagnostics.Debug.WriteLine("Mysql exception :- " + mysql.ToString());
            }
        }

        void DropContractTable()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(mysqlconnectionstring);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                MySqlCommand command = connection.CreateCommand();
                command.Connection = connection;
                command.CommandText = "DELETE FROM files.contract;";
                command.ExecuteNonQuery();
                connection.Clone();

            }
            catch (MySqlException mysql)
            {

                MessageBox.Show("DropContractTable" + "::" + mysql.Message.ToString());
            }
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            readfile();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'filesDataSet.contract' table. You can move, or remove it, as needed.
            this.contractTableAdapter.Fill(this.filesDataSet.contract);

        }

        private void fillByToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.contractTableAdapter.FillBy(this.filesDataSet.contract);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }




    }
}
