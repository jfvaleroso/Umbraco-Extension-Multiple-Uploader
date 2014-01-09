using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.editorControls.multipleUploader
{
    public class RelatedLinksDataTypeGeoff : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
        private umbraco.interfaces.IDataEditor _Editor;
        private umbraco.interfaces.IData _baseData;
        private MultipleUploaderPrevalueEditors _prevalueeditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {

                if (_Editor == null)
                    _Editor = new MultipleUploaderDataEditors(Data, ((MultipleUploaderPrevalueEditors)PrevalueEditor).Configuration);
                return _Editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new MultipleUploaderData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid("3AB1831F-CC6F-44C9-AD09-97AE60AAE5B4"); }
          
        }


        public override string DataTypeName
        {
            get { return "Multiple File Uploader V2.0.0"; }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new MultipleUploaderPrevalueEditors(this);
                return _prevalueeditor;
            }
        }
    }
}
