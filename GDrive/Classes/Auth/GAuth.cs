using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Util;


namespace GDrive.Classes.Auth
{
    public class GAuth
    {
        //public static MainWindow mainWindow;
        /// <summary>
        /// The remote service on which all the requests are executed.
        /// </summary>
        public static DriveService _service { get; private set; }

        /// <summary>
        /// Creates a concrete instance of the IAuthenticator to authenticate the user against Google OAuth2.0
        /// </summary>
        /// <returns></returns>
        public static IAuthenticator CreateAuthenticator()
        {
            var provider = new NativeApplicationClient(GoogleAuthenticationServer.Description);
            provider.ClientIdentifier = ClientCredentials.CLIENT_ID;
            provider.ClientSecret = ClientCredentials.CLIENT_SECRET;
            return new OAuth2Authenticator<NativeApplicationClient>(provider, GetAuthorization);
        }

        /// <summary>
        /// Method to get the authorization from the user to access their Google Drive from the application
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IAuthorizationState GetAuthorization(NativeApplicationClient client)
        {
            // You should use a more secure way of storing the key here as
            // .NET applications can be disassembled using a reflection tool.
            const string STORAGE = "gdrive_uploader";
            const string KEY = "z},drdzf11x9;87";
            string scope = DriveService.Scopes.Drive.GetStringValue();

            // Check if there is a cached refresh token available.
            IAuthorizationState state = AuthorizationMgr.GetCachedRefreshToken(STORAGE, KEY);
            if (state != null)
            {
                try
                {
                    client.RefreshToken(state);
                    return state; // Yes - we are done.
                }
                catch (DotNetOpenAuth.Messaging.ProtocolException ex)
                {
                    Debug.WriteLine("Using existing refresh token failed: " + ex.Message);
                }
            }

            // If we get here, there is no stored token. Retrieve the authorization from the user.
            state = AuthorizationMgr.RequestNativeAuthorization(client, scope);
            AuthorizationMgr.SetCachedRefreshToken(STORAGE, KEY, state);
            return state;
        }

        /// <summary>
        /// This is the worker method that executes when the user clicks the GO button.
        /// It illustrates the workflow that would need to take place in an actual application.
        /// </summary>
        public static void AuthorizeAndUpload()
        {

            // First, create a reference to the service you wish to use.
            // For this app, it will be the Drive service. But it could be Tasks, Calendar, etc.
            // The CreateAuthenticator method is passed to the service which will use that when it is time to authenticate
            // the calls going to the service.
            _service = new DriveService(CreateAuthenticator());

            // Open a dialog box for the user to pick a file.
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.Multiselect = false;
            dialog.ShowDialog();


            File body = new File();
            body.Title = System.IO.Path.GetFileName(dialog.FileName);
            body.Description = "A test document";
            body.MimeType = "text/plain";

            System.IO.Stream fileStream = dialog.OpenFile();
            byte[] byteArray = new byte[fileStream.Length];
            fileStream.Read(byteArray, 0, (int)fileStream.Length);

            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            // Get a listing of the existing files...
            List<File> fileList = Utilities.RetrieveAllFiles(_service);

            // Set a flag to keep track of whether the file already exists in the drive
            bool fileExists = false;

            foreach (File item in fileList)
            {
                if (item.Title == body.Title)
                {
                    // File exists in the drive already!
                    fileExists = true;
                    DialogResult result = MessageBox.Show("The file you picked already exists in your Google Drive. Do you wish to overwrite it?", "Confirmation", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {

                        // Yes... overwrite the file
                        Utilities.UpdateFile(_service, item.Id, item.Title, item.Description, item.MimeType, dialog.FileName, true);
                    }

                    else if (result == DialogResult.No)
                    {

                        // MessageBoxResult.No code here
                        Utilities.InsertFile(_service, System.IO.Path.GetFileName(dialog.FileName), "An uploaded document", "", "text/plain", dialog.FileName);
                    }

                    else
                    {

                        // MessageBoxResult.Cancel code here
                        return;
                    }
                    break;
                }
            }

            // Check to see if the file existed. If not, it is a new file and must be uploaded.
            if (!fileExists)
            {
                Utilities.InsertFile(_service, System.IO.Path.GetFileName(dialog.FileName), "An uploaded document", "", "text/plain", dialog.FileName);
            }

            MessageBox.Show("Upload Complete");
        }


    }
}
