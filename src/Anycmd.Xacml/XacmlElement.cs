using System;
using System.Xml;

namespace Anycmd.Xacml
{
    /// <summary>
    /// ����xacmlԪ�صĻ����͡�
    /// </summary>
    public abstract class XacmlElement
    {
        #region Private members

        private readonly XacmlVersion _schemaVersion;
        private readonly XacmlSchema _schema;

        #endregion

        #region Constructor

        /// <summary>
        /// Ĭ�ϳ�ʼ������
        /// </summary>
        /// <param name="schema">������֤�ĵ���ö��</param>
        /// <param name="schemaVersion">���ڼ���xacmlԪ�ص�ģʽ�İ汾</param>
        protected XacmlElement(XacmlSchema schema, XacmlVersion schemaVersion)
        {
            _schema = schema;
            _schemaVersion = schemaVersion;
        }

        /// <summary>
        /// �չ�������
        /// </summary>
        protected XacmlElement()
        {
        }
        #endregion

        #region Public properties

        /// <summary>
        /// ������֤�ĵ���ö�١�
        /// </summary>
        public XacmlVersion SchemaVersion
        {
            get { return _schemaVersion; }
        }

        /// <summary>
        /// ������xacmlԪ�ص�ģʽ��Schema����
        /// </summary>
        public XacmlSchema Schema
        {
            get { return _schema; }
        }

        /// <summary>
        /// �鿴��ǰʵ���Ƿ���ֻ���汾��
        /// </summary>
        public abstract bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// ���ظ��ݵ�ǰģʽ�Ͱ汾��ָ���������ռ��ַ�����
        /// </summary>
        internal protected string XmlDocumentSchema
        {
            get
            {
                switch (_schema)
                {
                    case XacmlSchema.Context:
                        switch (this.SchemaVersion)
                        {
                            case XacmlVersion.Version11:
                            case XacmlVersion.Version10:
                                return Consts.Schema1.Namespaces.Context;
                                break;
                            case XacmlVersion.Version20:
                                return Consts.Schema2.Namespaces.Context;
                                break;
                            default:
                                throw new EvaluationException("����İ汾��" + SchemaVersion);
                        }
                        break;
                    case XacmlSchema.Policy:
                        switch (this.SchemaVersion)
                        {
                            case XacmlVersion.Version11:
                            case XacmlVersion.Version10:
                                return Consts.Schema1.Namespaces.Policy;
                                break;
                            case XacmlVersion.Version20:
                                return Consts.Schema2.Namespaces.Policy;
                                break;
                            default:
                                throw new EvaluationException("����İ汾��" + SchemaVersion);
                        }
                        break;
                    default:
                        throw new EvaluationException("�����ģʽ" + _schema);
                }
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// ���ݸ����İ汾����֤������ģʽ��
        /// </summary>
        /// <param name="reader">���ڶ�ȡ�����ռ�</param>
        /// <param name="version">������֤�汾��</param>
        /// <returns><c>true</c>, ��������ռ���ȷ</returns>
        protected bool ValidateSchema(XmlReader reader, XacmlVersion version)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            switch (_schema)
            {
                case XacmlSchema.Policy:
                    switch (version)
                    {
                        case XacmlVersion.Version10:
                        case XacmlVersion.Version11:
                            return (reader.NamespaceURI == Consts.Schema1.Namespaces.Policy);
                        case XacmlVersion.Version20:
                            return (reader.NamespaceURI == Consts.Schema2.Namespaces.Policy);
                        default:
                            throw new EvaluationException("����İ汾��" + version);
                    }
                    break;
                case XacmlSchema.Context:
                    switch (version)
                    {
                        case XacmlVersion.Version10:
                        case XacmlVersion.Version11:
                            return (reader.NamespaceURI == Consts.Schema1.Namespaces.Context);
                        case XacmlVersion.Version20:
                            return (reader.NamespaceURI == Consts.Schema2.Namespaces.Context);
                        default:
                            throw new EvaluationException("����İ汾��" + version);
                    }
                    break;
                default:
                    throw new EvaluationException("�����ģʽ" + _schema);
            }
        }

        #endregion
    }
}
