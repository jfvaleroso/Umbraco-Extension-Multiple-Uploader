using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using umbraco.interfaces;
using umbraco.editorControls;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.IO;
using System.Web;
using System.IO;
using umbraco.cms.businesslogic.member;


namespace umbraco.editorControls.multipleUploader
{
    [ValidationProperty("IsValid")]
    public class MultipleUploaderDataEditors : UpdatePanel, IDataEditor
    {
        private umbraco.interfaces.IData _data;
        string _configuration;
        private string[] config;

        private ListBox _listboxLinks;
        private Button _buttonUp;
        private Button _buttonDown;
        private Button _downloadLnk;
        private Button _buttonDelete;
        private TextBox _textboxLinkTitle;
        private CheckBox _checkNewWindow;
        private TextBox _textBoxExtUrl;
        private Button _buttonAddExtUrl;
        private Button _buttonAddIntUrlCP;
        private XmlDocument _xml;
        private FileUpload _fileUpload;
        private PostBackTrigger _postBackTrigger;
        private Label _notification;
        private bool _fileTooLarge=false;
        private bool _allowFile = false;
        private bool _validPath = true;

        private pagePicker _pagePicker;
        private PagePickerDataExtractor _pagePickerExtractor;

        public MultipleUploaderDataEditors(umbraco.interfaces.IData Data, string Configuration)
        {
            _data = Data;
            _configuration = Configuration;

            config = _configuration.Split("|".ToCharArray());
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        /// <summary>
        /// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
        /// </summary>
        /// <value>Am I valid?</value>
        public string IsValid
        {
            get
            {
                if (_listboxLinks != null)
                {
                    if (_listboxLinks.Items.Count > 0)
                        return "Valid";
                }
                return "";
            }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public Control Editor { get { return this; } }

        //Creates and saves a xml format of the content of the _listboxLinks
        // <links>
        //    <link type="external" title="google" link="http://google.com" newwindow="1" />
        //    <link type="internal" title="home" link="1234" newwindow="0" />
        // </links>
        //We could adapt the global xml at every adjustment, but this implementation is easier
        // (and possibly more efficient).
        public void Save()
        {
            XmlDocument doc = createBaseXmlDocument();
            XmlNode root = doc.DocumentElement;
            foreach (ListItem item in _listboxLinks.Items)
            {
                string value = item.Value;

                XmlNode newNode = doc.CreateElement("link");

                XmlNode titleAttr = doc.CreateNode(XmlNodeType.Attribute, "title", null);
                titleAttr.Value = item.Text;
                newNode.Attributes.SetNamedItem(titleAttr);

                XmlNode linkAttr = doc.CreateNode(XmlNodeType.Attribute, "link", null);
                linkAttr.Value = value.Substring(3);
                newNode.Attributes.SetNamedItem(linkAttr);

              

                XmlNode typeAttr = doc.CreateNode(XmlNodeType.Attribute, "type", null);
                if (value.Substring(0, 1).Equals("i"))
                    typeAttr.Value = "internal";
                else
                    typeAttr.Value = "external";
                newNode.Attributes.SetNamedItem(typeAttr);

                XmlNode windowAttr = doc.CreateNode(XmlNodeType.Attribute, "newwindow", null);
                if (value.Substring(1, 1).Equals("n"))
                    windowAttr.Value = "1";
                else
                    windowAttr.Value = "0";
                newNode.Attributes.SetNamedItem(windowAttr);


                XmlNode repositoryAttr = doc.CreateNode(XmlNodeType.Attribute, "repository", null);
                if (value.Substring(2, 1).Equals("s"))
                    repositoryAttr.Value = "Shared Path";
                else
                    repositoryAttr.Value = "Local Site Path";
                newNode.Attributes.SetNamedItem(repositoryAttr);


                root.AppendChild(newNode);
            }

            this._data.Value = doc.InnerXml;
        }

        //Draws the controls, only gets called for the first drawing of the page, not for each postback
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);



            try
            {
                _xml = new XmlDocument();
                _xml.LoadXml(_data.Value.ToString());

            }
            catch
            {
                _xml = createBaseXmlDocument();
            }

            _listboxLinks = new ListBox();
            _listboxLinks.ID = "links" + base.ID;
            _listboxLinks.Width = 400;
            _listboxLinks.Height = 140;
            foreach (XmlNode node in _xml.DocumentElement.ChildNodes)
            {
                string text = node.Attributes["title"].Value.ToString();
                string value = 
                    (node.Attributes["type"].Value.ToString().Equals("internal") ? "i" : "e")
                    + (node.Attributes["newwindow"].Value.ToString().Equals("1") ? "n" : "o")
                    + (node.Attributes["repository"].Value.ToString().Equals("Shared Path") ? "s" : "l")
                    + node.Attributes["link"].Value.ToString();
                _listboxLinks.Items.Add(new ListItem(text, value));
            }

            _buttonUp = new Button();
            _buttonUp.ID = "btnUp" + base.ID;
            _buttonUp.Text = umbraco.ui.GetText("relatedlinks", "modeUp");
            _buttonUp.Width = 80;
            _buttonUp.Click += new EventHandler(this.buttonUp_Click);


            _buttonDown = new Button();
            _buttonDown.ID = "btnDown" + base.ID;
            _buttonDown.Attributes.Add("style", "margin-top: 5px;");
            _buttonDown.Text = umbraco.ui.GetText("relatedlinks", "modeDown");
            _buttonDown.Width = 80;
            _buttonDown.Click += new EventHandler(this.buttonDown_Click);

            _downloadLnk = new Button();
            _downloadLnk.ID = "btnDownload" + base.ID;
            _downloadLnk.Attributes.Add("style", "margin-top: 5px;");
            _downloadLnk.Text = umbraco.ui.GetText("relatedlinks", "modeDownload");
            _downloadLnk.Width = 80;
            _downloadLnk.Click += new EventHandler(this.downloadLnk_Click);

            _buttonDelete = new Button();
            _buttonDelete.ID = "btnDel" + base.ID;
            _buttonDelete.Text = umbraco.ui.GetText("relatedlinks", "removeLink");
            _buttonDelete.Width = 80;
            _buttonDelete.Click += new EventHandler(this.buttonDel_Click);

            _textboxLinkTitle = new TextBox();
            _textboxLinkTitle.Width = 400;
            _textboxLinkTitle.ID = "linktitle" + base.ID;

            _checkNewWindow = new CheckBox();
            _checkNewWindow.ID = "checkNewWindow" + base.ID;
            _checkNewWindow.Checked = false;
            _checkNewWindow.Text = umbraco.ui.GetText("relatedlinks", "newWindow");





            _textBoxExtUrl = new TextBox();
            _textBoxExtUrl.Width = 400;
            _textBoxExtUrl.ID = "exturl" + base.ID;

            _buttonAddExtUrl = new Button();
            _buttonAddExtUrl.ID = "btnAddExtUrl" + base.ID;
            _buttonAddExtUrl.Text = umbraco.ui.GetText("relatedlinks", "addlink");
            _buttonAddExtUrl.Width = 80;
            _buttonAddExtUrl.Click += new EventHandler(this.buttonAddExt_Click);

            _buttonAddIntUrlCP = new Button();
            _buttonAddIntUrlCP.ID = "btnAddIntUrl" + base.ID;
            _buttonAddIntUrlCP.Text = umbraco.ui.GetText("relatedlinks", "addlink");
            _buttonAddIntUrlCP.Width = 80;
            _buttonAddIntUrlCP.Click += new EventHandler(this.buttonAddIntCP_Click);

            _fileUpload = new FileUpload();
            _fileUpload.ID = "fileupload" + base.ID;

            _postBackTrigger = new PostBackTrigger();
            _postBackTrigger.ControlID = _buttonAddIntUrlCP.ID;

            _notification = new Label();
            _notification.ID = "notification" + base.ID;
            _notification.Text = "";
      



            _pagePickerExtractor = new PagePickerDataExtractor();
            _pagePicker = new pagePicker(_pagePickerExtractor);
            _pagePicker.ID = "pagePicker" + base.ID;

            ContentTemplateContainer.Controls.Add(new LiteralControl("<div class=\"relatedlinksdatatype\" style=\"text-align: left;  padding: 5px;\"><table><tr><td rowspan=\"2\">"));
            ContentTemplateContainer.Controls.Add(_listboxLinks);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td><td style=\"vertical-align: top\">"));
            ContentTemplateContainer.Controls.Add(_buttonUp);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(_buttonDown);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(_downloadLnk);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td></tr><tr><td style=\"vertical-align: bottom\">"));
            ContentTemplateContainer.Controls.Add(_buttonDelete);
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(new LiteralControl("</td></tr></table>"));

            // Add related links container
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addExternalLinkPanel').style.display='none';document.getElementById('{0}_addExternalLinkButton').style.display='none';document.getElementById('{0}_addLinkContainer').style.display='block';document.getElementById('{0}_addInternalLinkPanel').style.display='block';document.getElementById('{0}_addInternalLinkButton').style.display='block';\"><strong>{1}</strong></a>", ClientID, umbraco.ui.GetText("relatedlinks", "addInternalFile"))));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format(" | <a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addInternalLinkPanel').style.display='none';document.getElementById('{0}_addInternalLinkButton').style.display='none';document.getElementById('{0}_addLinkContainer').style.display='block';document.getElementById('{0}_addExternalLinkPanel').style.display='block';document.getElementById('{0}_addExternalLinkButton').style.display='block';\"><strong>{1}</strong></a>", ClientID, umbraco.ui.GetText("relatedlinks", "addExternalFile"))));
            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(_notification);

            // All urls
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addLinkContainer\" style=\"display: none; padding: 4px; border: 1px solid #ccc; margin-top: 5px;margin-right:10px;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<a href=\"javascript:;\" onClick=\"document.getElementById('{0}_addLinkContainer').style.display='none';\" style=\"border: none;\"><img src=\"{1}/images/close.png\" style=\"float: right\" /></a>", ClientID, this.Page.ResolveUrl(SystemDirectories.Umbraco))));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "caption"))));
            ContentTemplateContainer.Controls.Add(_textboxLinkTitle);

            //add postback trigger for upload postback
            this.Triggers.Add(_postBackTrigger);


            ContentTemplateContainer.Controls.Add(new LiteralControl("<br />"));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addExternalLinkPanel\" style=\"display: none; margin: 3px 0\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "linkurl"))));
      
          
            ContentTemplateContainer.Controls.Add(_textBoxExtUrl);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addInternalLinkPanel\" style=\"display: none; margin: 3px 0\">", ClientID)));
            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("{0}:<br />", umbraco.ui.GetText("relatedlinks", "file"))));

            
            ContentTemplateContainer.Controls.Add(_fileUpload);
           
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

 
            ContentTemplateContainer.Controls.Add(new LiteralControl("<div style=\"margin: 5px 0\">"));
            ContentTemplateContainer.Controls.Add(_checkNewWindow);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));


            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addInternalLinkButton\" style=\"display: none;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(_buttonAddIntUrlCP);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl(String.Format("<div id=\"{0}_addExternalLinkButton\" style=\"display: none;\">", ClientID)));
            ContentTemplateContainer.Controls.Add(_buttonAddExtUrl);
            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            ContentTemplateContainer.Controls.Add(new LiteralControl("</div>"));

            resetInputMedia();
        }


        private XmlDocument createBaseXmlDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("links");
            doc.AppendChild(root);
            return doc;
        }

        private void buttonUp_Click(Object o, EventArgs ea)
        {
            int index = _listboxLinks.SelectedIndex;
            if (index > 0) //not the first item
            {
                ListItem temp = _listboxLinks.SelectedItem;
                _listboxLinks.Items.RemoveAt(index);
                _listboxLinks.Items.Insert(index - 1, temp);
                _listboxLinks.SelectedIndex = index - 1;
               
            }
        }
        private void buttonDown_Click(Object o, EventArgs ea)
        {
            int index = _listboxLinks.SelectedIndex;
            if (index > -1 && index < _listboxLinks.Items.Count - 1) //not the last item
            {
                ListItem temp = _listboxLinks.SelectedItem;
                _listboxLinks.Items.RemoveAt(index);
                _listboxLinks.Items.Insert(index + 1, temp);
                _listboxLinks.SelectedIndex = index + 1;
                
            }
        }
         private void  downloadLnk_Click(Object o, EventArgs ea)
        {

            int index = _listboxLinks.SelectedIndex;
            if (index > -1)
            {
                ListItem item = _listboxLinks.SelectedItem;
                HttpContext.Current.Response.Redirect(item.Value.Substring(3));
            }
    }

        private void buttonDel_Click(Object o, EventArgs ea)
        {
          
           
            string path = _listboxLinks.SelectedValue.ToString();

            if (!String.IsNullOrEmpty(path))
            {

                string docType = path[0].ToString();
                string repositoryType = path[2].ToString();
                //delete only internal files
                if (docType == "i")
                {
                    string str = path.Substring(3);
                    deleteFile(str, repositoryType);

                }

                int index = _listboxLinks.SelectedIndex;
                if (index > -1)
                {
                    _listboxLinks.Items.RemoveAt(index);
                    Save();
                }
            }
            else
            {
                _notification.Text = "Please select the file that you want to delete.";
            }

        }
        private void buttonAddExt_Click(Object o, EventArgs ea)
        {
            _notification.Text = "";
            string url = _textBoxExtUrl.Text.Trim();
            if (url.Length > 0 && _textboxLinkTitle.Text.Length > 0)
            {
                // use default HTTP protocol if no protocol was specified
                if (!(url.Contains("://")))
                {
                    url = "http://" + url;
                }

                string value = "e" + (_checkNewWindow.Checked ? "n" : "o") +(config[0] == "Shared Path" ? "s" : "l") + url;
                _listboxLinks.Items.Add(new ListItem(_textboxLinkTitle.Text, value));
                resetInputMedia();
                Save();
            }
            else
            {
                _notification.Text = "Please provide a caption for your link.";
            }
        }
        private void buttonAddIntCP_Click(Object o, EventArgs ea)
        {
            //instantiate UPloader

            _notification.Text = "";
            _allowFile = allowedFile();
            // _pagePicker.Save();
            if (!String.IsNullOrEmpty(_textboxLinkTitle.Text) && _fileUpload.HasFile)
            //&& _pagePickerExtractor.Value != null
            //&& _pagePickerExtractor.Value.ToString() != "")
            {
                string value = "i" + (_checkNewWindow.Checked ? "n" : "o") + (config[0] == "Shared Path" ? "s" : "l") + saveFile(_fileUpload.PostedFile, _fileUpload);
                if (!_fileTooLarge && _allowFile && _validPath)
                {
                    _listboxLinks.Items.Add(new ListItem(_textboxLinkTitle.Text, value));
                    resetInputMedia();
                    Save();
                }
                //  ScriptManager.RegisterClientScriptBlock(_pagePicker, _pagePicker.GetType(), "clearPagePicker", _pagePicker.ClientID + "_clear();", true);
            }
            else
            {
                _notification.Text = "Please provide a caption for your file.";
            }

        }
        private void resetInputMedia()
        {

            _textBoxExtUrl.Text = "http://";
            _textboxLinkTitle.Text = "";
            _pagePickerExtractor.Value = "";


        }


      


        public string saveFile(HttpPostedFile file, FileUpload fileUploader)
        {
            if (_allowFile)
            {

                string repository = config[0];
                string basePath = config[1];
                string maxFileSize = config[2];
                DateTime now = DateTime.Now;
                string year = now.Year.ToString();
                string month = now.Month.ToString();
                string day = now.Day.ToString();
                string DateAttribute = year + "/" + month + "/" + day + "/";
                string memberAttribute = GetCurrentMemberGUID() + "/";
                long defaultSize = 0;
                bool isNum = long.TryParse(maxFileSize, out defaultSize);
                double origSize = 0;

                if (!isNum)
                {
                    defaultSize = 1048576;
                    origSize = 1024;//1MB
                }
                else
                {
                    defaultSize = Convert.ToInt32(maxFileSize) * 1024;
                    origSize = Convert.ToDouble(maxFileSize);
                }


                long _uploadFileSize = _fileUpload.PostedFile.ContentLength;

                try
                {
                    if (!(_uploadFileSize > defaultSize))
                    {

                        //create direc
                        if (sharedPathRepository)
                        { Directory.CreateDirectory(basePath + memberAttribute); }
                        else //local server
                        { System.IO.Directory.CreateDirectory(IOHelper.MapPath("~/" + basePath + "/" + memberAttribute)); }


                        string path = basePath + memberAttribute;
                        string filename = fileUploader.FileName;

                        string pathToCheck = path + filename;
                        string tempfilename = "";



                        if (checkIfFileExist(pathToCheck))
                        {
                            int counter = 2;
                            while (checkIfFileExist(pathToCheck))
                            {

                                tempfilename = counter.ToString() + filename;
                                pathToCheck = path + tempfilename;
                                counter++;
                            }

                             if (sharedPathRepository)
                            {
                                fileUploader.SaveAs(pathToCheck);

                            }
                            else
                            {
                                fileUploader.SaveAs(HttpContext.Current.Server.MapPath("~/" + pathToCheck));
                            }
                            _notification.Text = "file successfully uploaded.";
                            return pathToCheck;

                        }
                        else
                        {

                            if (sharedPathRepository)
                            { fileUploader.SaveAs(pathToCheck); }
                            else
                            { fileUploader.SaveAs(HttpContext.Current.Server.MapPath("~/" + pathToCheck)); }
                            _notification.Text = "file successfully uploaded.";
                            return pathToCheck;
                        }





                    }
                    else
                    {
                        _fileTooLarge = true;
                        _notification.Text = "File too large, maximum file size is " + origSize.ToString() + " KB";
                        return "File too large";
                    }

                }
                catch
                {
                    _validPath = false;
                    _notification.Text = "Erro uploading the file: Be sure your repository is correctly configured.";
                    return String.Empty;

                }
            }
            else
            {
                _notification.Text = "Your file is not allowed to be uploaded. Allowed file type: " + config[3].ToString();
                return String.Empty;
            }

        }
        public void deleteFile(string path, string repository)
        {
            
            if (!String.IsNullOrEmpty(path))
            {
                if (repository == "s")
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        _notification.Text = "file successfully deleted";
                    }
                }
                else //local server
                {
                    if (File.Exists(IOHelper.MapPath("~/" + path)))
                    {
                        File.Delete(IOHelper.MapPath("~/" + path));
                        _notification.Text = "file successfully deleted";
                    }

                }

            }
            else
            {

            }


        }

        public bool allowedFile()
        {
            string extension= Path.GetExtension(_fileUpload.FileName);
            string values= config[3];
            bool final = false;
            if (!String.IsNullOrEmpty(values))
            {
                string[] allowedFileExtension = values.Split(',');
                foreach (string s in allowedFileExtension)
                {
                    if (s == extension || s==extension.ToLower() || s==extension.ToUpper())
                    {
                        final = true;
                        break;

                    }
                 
                }
                return final;

            }
            else
            {
                return false;
            }

           

        
        }
        public bool sharedPathRepository
        {
            get
            {
                if (config[0] == "Shared Path")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


        }

        public bool checkIfFileExist(string pathToCheck)
        {
                bool check;
                if (sharedPathRepository)
                {
                    check=System.IO.File.Exists(pathToCheck);
                    return check;
                }
                else
                {
                    check = System.IO.File.Exists(IOHelper.MapPath("~/" + pathToCheck));
                    return check;
                }
            
            
        
        }

        public string GetCurrentMemberGUID()
        {
            int memberId = Convert.ToInt32(HttpContext.Current.Request["id"]);
            Member m = new Member(memberId);
            string guid = m.getProperty("guid").Value.ToString();
            string newGuid = "";
            if (string.IsNullOrEmpty(guid))
            {
                newGuid = Guid.NewGuid().ToString();
                m.getProperty("guid").Value = newGuid;
                m.Save();
            }
            return guid;
        }
       





    }


}
