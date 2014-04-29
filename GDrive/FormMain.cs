using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using GDrive.Classes;
using GDrive.Classes.Auth;

using DotNetOpenAuth.OAuth2;

using ImportedFromMono.System.Web;
using ImportedFromMono.System.Web.Util;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;


using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;


using Google;
using Google.Apis;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Logging;
using Google.Apis.Discovery;
using Google.Apis.Discovery.Schema;
using Google.Apis.Requests;
using Google.Apis.Util;
using Google.Apis.Json;
using Google.Apis.Testing;
using Google.Apis.Upload;

namespace GDrive
{
    public partial class FormMain : Form
    {

        private DriveService m_DriveService;


        private string[] SCOPES = new string[]
        {
            "https://www.googleapis.com/auth/drive.file",
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/drive.install"
        };

        public FormMain()
        {
            InitializeComponent();

            m_DriveService = new DriveService(GAuth.CreateAuthenticator());

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Clear();



                FilesResource.ListRequest myList = m_DriveService.Files.List();

                FileList myFiles = myList.Fetch();



                foreach (Google.Apis.Drive.v2.Data.File myFile in myFiles.Items)
                {
                    string myType = "File";

                    if (myFile.MimeType == "application/vnd.google-apps.folder")
                    {
                        myType = "Folder";
                    }

                    ;

                    listBox1.Items.Add(string.Format("{0}: {1}", myType, myFile.Title));


                }





            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
                
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //GAuth.AuthorizeAndUpload();


        }

    }
}
