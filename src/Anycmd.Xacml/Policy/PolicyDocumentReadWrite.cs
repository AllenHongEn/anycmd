
namespace Anycmd.Xacml.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// ��ʾһ���ɶ�д�İ���һ�����Ի���Լ��Ĳ����ĵ���
    /// </summary>
    public class PolicyDocumentReadWrite
    {
        /// <summary>
        /// The compiled schema for the policy document is kept in memory for performance reasons.
        /// </summary>
        private static XmlSchemaSet _compiledSchemas11;
        private static XmlSchemaSet _compiledSchemas20;

        #region Private members

        /// <summary>
        /// The PolicySet if the document defines a PolicySet.
        /// </summary>
        private PolicySetElementReadWrite _policySet;

        /// <summary>
        /// The Policy if the document defines a single Policy.
        /// </summary>
        private PolicyElementReadWrite _policy;

        /// <summary>
        /// Whether the Xsd validation for the have succeeded or not.
        /// </summary>
        private bool _isValidDocument = true;

        /// <summary>
        /// All the namespaces and the prefix defined in the document. This is used in the XPath
        /// queries because the XPath uses the preffixes and we must provide them in the 
        /// XmlNamespaceManager.
        /// </summary>
        private IDictionary<string, string> _namespaces = new Dictionary<string, string>();

        /// <summary>
        /// The name of the embedded resource for the 1.0 schema.
        /// </summary>
        public const string Xacml10PolicySchemaResourceName = "Anycmd.Xacml.Schemas.cs-xacml-schema-policy-01.xsd";

        /// <summary>
        /// The name of the embedded resource for the 2.0 schema.
        /// </summary>
        public const string Xacml20PolicySchemaResourceName = "Anycmd.Xacml.Schemas.access_control-xacml-2.0-policy-schema-os.xsd";

        /// <summary>
        /// The version of the instance used to validate this document.
        /// </summary>
        private XacmlVersion _schemaVersion;
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new blank PolicyDocument
        /// </summary>
        /// <param name="schemaVersion">The version of the schema that will be used to validate.</param>
        public PolicyDocumentReadWrite(XacmlVersion schemaVersion)
        {
            _schemaVersion = schemaVersion;
        }

        /// <summary>
        /// Creates a new PolicyDocument using the XmlReader instance provided with the possibility of writing.
        /// </summary>
        /// <param name="reader">The XmlReader positioned at the begining of the document.</param>
        /// <param name="schemaVersion">The version of the schema that will be used to validate.</param>
        public PolicyDocumentReadWrite(XmlReader reader, XacmlVersion schemaVersion)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            _schemaVersion = schemaVersion;

            // Prepare Xsd validation
            var validationHandler = new ValidationEventHandler(vreader_ValidationEventHandler);
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            settings.ValidationEventHandler += validationHandler;
            XmlReader vreader = null;
            try
            {
                switch (schemaVersion)
                {
                    case XacmlVersion.Version10:
                    case XacmlVersion.Version11:
                        {
                            if (_compiledSchemas11 == null)
                            {
                                Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Xacml10PolicySchemaResourceName);
                                _compiledSchemas11 = new XmlSchemaSet();
                                Debug.Assert(schemaStream != null, "schemaStream != null");
                                _compiledSchemas11.Add(XmlSchema.Read(schemaStream, validationHandler));
                                _compiledSchemas11.Compile();
                            }
                            settings.Schemas.Add(_compiledSchemas11);
                            break;
                        }
                    case XacmlVersion.Version20:
                        {
                            if (_compiledSchemas20 == null)
                            {
                                Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Xacml20PolicySchemaResourceName);
                                _compiledSchemas20 = new XmlSchemaSet();
                                Debug.Assert(schemaStream != null, "schemaStream != null");
                                _compiledSchemas20.Add(XmlSchema.Read(schemaStream, validationHandler));
                                _compiledSchemas20.Compile();
                            }
                            settings.Schemas.Add(_compiledSchemas20);
                            break;
                        }
                    default:
                        throw new ArgumentException("�����xacmlģʽ�汾��:" + schemaVersion);
                }
                vreader = XmlReader.Create(reader, settings);
                // Read and validate the document.
                while (vreader.Read())
                {
                    //Read all the namespaces and keep them in the _namespaces hashtable.
                    if (vreader.HasAttributes)
                    {
                        while (vreader.MoveToNextAttribute())
                        {
                            if (vreader.LocalName == Consts.Schema1.Namespaces.Xmlns)
                            {
                                _namespaces.Add(vreader.Prefix, vreader.Value);
                            }
                            else if (vreader.Prefix == Consts.Schema1.Namespaces.Xmlns)
                            {
                                _namespaces.Add(vreader.LocalName, vreader.Value);
                            }
                        }
                        vreader.MoveToElement();
                    }

                    // Check the first element of the document and proceeds to read the contents 
                    // depending on the first node name.
                    switch (vreader.LocalName)
                    {
                        case Consts.Schema1.PolicySetElement.PolicySet:
                            {
                                _policySet = new PolicySetElementReadWrite(vreader, schemaVersion);
                                return;
                            }
                        case Consts.Schema1.PolicyElement.Policy:
                            {
                                _policy = new PolicyElementReadWrite(vreader, schemaVersion);
                                return;
                            }
                    }
                }
            }
            finally
            {
                if (vreader != null)
                {
                    vreader.Close();
                }
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The version of the schema used to validate this instance.
        /// </summary>
        public virtual XacmlVersion Version
        {
            get { return _schemaVersion; }
            set { _schemaVersion = value; }
        }

        /// <summary>
        /// Whether the document have passed the Xsd validation.
        /// </summary>
        public virtual bool IsValidDocument
        {
            get { return _isValidDocument; }
            set { _isValidDocument = value; }
        }

        /// <summary>
        /// The PolicySet contained in the document.
        /// </summary>
        public virtual PolicySetElementReadWrite PolicySet
        {
            get { return _policySet; }
            set { _policySet = value; }
        }

        /// <summary>
        /// The Policy contained in the document.
        /// </summary>
        public virtual PolicyElementReadWrite Policy
        {
            get { return _policy; }
            set { _policy = value; }
        }

        /// <summary>
        /// All the namespaced defined in the document.
        /// </summary>
        public virtual IDictionary<string, string> Namespaces
        {
            get { return _namespaces; }
            set { _namespaces = value; }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// The method called for each Xsd error detected during validation.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The validation error detail.</param>
        private void vreader_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine();
            _isValidDocument = false;
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Writes the XML of the current element
        /// </summary>
        /// <param name="writer">The XmlWriter in which the element will be written</param>
        public void WriteDocument(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            writer.WriteStartDocument();

            //If there's a PolicySet element
            if (_policySet != null)
            {
                this._policySet.WriteDocument(writer, _namespaces);
            }
            //If there's a Policy element
            else if (_policy != null)
            {
                this._policy.WriteDocument(writer, _namespaces);
            }
            writer.WriteEndDocument();
        }

        #endregion
    }
}
