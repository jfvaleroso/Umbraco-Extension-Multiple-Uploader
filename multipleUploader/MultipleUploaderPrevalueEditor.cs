using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using umbraco.BusinessLogic;
using umbraco.editorControls;
using umbraco.DataLayer;

namespace umbraco.editorControls.multipleUploader
{
    public class MultipleUploaderPrevalueEditors : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {
        public ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        #region IDataPrevalue Members

        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        private DropDownList _dropdownlist;
        private TextBox _textBox;
        private TextBox _fileMaxLength;
        private TextBox _allowedFile;
        //private CheckBox _showUrls;

        public MultipleUploaderPrevalueEditors(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
        {

            _datatype = DataType;
            setupChildControls();

        }

        private void setupChildControls()
        {
            _dropdownlist = new DropDownList();
            _dropdownlist.ID = "dbtype";
            _dropdownlist.Items.Add("Shared Path");
            _dropdownlist.Items.Add("Local Site Path");
            _dropdownlist.Width = 300;

            _textBox = new TextBox();
            _textBox.ID = "path";
            _textBox.Width = 300;

            _fileMaxLength = new TextBox();
            _fileMaxLength.ID = "fileMaxLength";
            _textBox.Width = 300;

            _allowedFile = new TextBox();
            _allowedFile.ID = "allowedFile";
            _allowedFile.Width = 300;



            Controls.Add(_dropdownlist);
            Controls.Add(_textBox);
            Controls.Add(_fileMaxLength);
            Controls.Add(_allowedFile);
            
           
        }



        public Control Editor
        {
            get
            {
                return this;
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                string[] config = Configuration.Split("|".ToCharArray());
                if (config.Length > 1)
                {
                    //_showUrls.Checked = Convert.ToBoolean(config[0]);
                    _dropdownlist.SelectedValue = config[0];
                    _textBox.Text = config[1];
                    _fileMaxLength.Text = config[2];
                    _allowedFile.Text = config[3];

                }
               
            }
        }

        public void Save()
        {
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);

            // Generate data-string
            string data = _dropdownlist.SelectedValue + "|" + _textBox.Text + "|" + _fileMaxLength.Text + "|" + _allowedFile.Text ;

            // If the add new prevalue textbox is filled out - add the value to the collection.
            IParameter[] SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlParams);
            // need to unlock the parameters (for SQL CE compat)
            SqlParams = new IParameter[] {
										SqlHelper.CreateParameter("@value",data),
										SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", SqlParams);


        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("<table style='width:100%'>");
            writer.WriteLine("<tr><th>Repository Type:</th><td>");
            _dropdownlist.RenderControl(writer);
            writer.Write("</td></tr>");

            writer.WriteLine("<tr><th>Path:</th><td>");
            _textBox.RenderControl(writer);
            writer.WriteLine("<br/><span style='font-size:.9em'>Ex. Shared Path: \\\\network\\folder$\\ | Local Site Path(folder within your site): media/ftp/</span>");
            writer.Write("</td></tr>");

            writer.WriteLine("<tr><th>Max file size:</th><td>");
            _fileMaxLength.RenderControl(writer);
            writer.WriteLine("KB");
            writer.WriteLine("<br/><span style='font-size:.9em'>Default max file size is 1000KB(1MB).</span>");
            writer.WriteLine("<br/><span style='font-size:.9em'>Note: Add your max file size in web.config as maxRequestLength=5000KB(5MB)</span>");
            writer.Write("</td></tr>");


            writer.WriteLine("<tr><th>Allowed files:</th><td>");
           _allowedFile.RenderControl(writer);
            writer.WriteLine("<br/><span style='font-size:.9em'>Add allowed file extensions(ex. jpg,.doc,.docx).</span>");     
            writer.Write("</td></tr>");
            writer.Write("</table>");
        }

        public string Configuration
        {
            get
            {
                object conf =
                     SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                                             SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                if (conf != null)
                    return conf.ToString();
                else
                    return "";

            }
        }

        #endregion
    }
}
