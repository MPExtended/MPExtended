//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
// Additional fixes from http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/

using System;
using System.Xml;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
//using MPExtended.Libraries.Service;


namespace Microsoft.ServiceModel.Samples
{
    //This is the binding element that, when plugged into a custom binding, will enable the GZip encoder
    // Removed IPolicyExportExtension as it's not used directly by app.config
    public sealed class GZipMessageEncodingBindingElement 
                        : MessageEncodingBindingElement //BindingElement
    {

        //We will use an inner binding element to store information required for the inner encoder
        MessageEncodingBindingElement innerBindingElement;

        //By default, use the default text encoder as the inner encoder
        public GZipMessageEncodingBindingElement()
            : this(new TextMessageEncodingBindingElement()) { }

        public GZipMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)
        {
            //Log.Debug(GetType().FullName + "::ctor with " + messageEncoderBindingElement.GetType().FullName);
            this.innerBindingElement = messageEncoderBindingElement;
        }

        public MessageEncodingBindingElement InnerMessageEncodingBindingElement
        {
            get {
				//Log.Debug(GetType().FullName + "::InnerMessageEncodingBindingElement");
				return innerBindingElement;
			}
            set {
				//Log.Debug(GetType().FullName + "::InnerMessageEncodingBindingElement, " + value.GetType().FullName);
				innerBindingElement = value;
			}
        }

        //Main entry point into the encoder binding element. Called by WCF to get the factory that will create the
        //message encoder
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            //Log.Debug(GetType().FullName + "::CreateMessageEncoderFactory, " + innerBindingElement.CreateMessageEncoderFactory().GetType().FullName);
            return new GZipMessageEncoderFactory(innerBindingElement.CreateMessageEncoderFactory());
        }
       
        public override MessageVersion MessageVersion
        {
            get {
				//Log.Debug(GetType().FullName + "::MessageVersion " + innerBindingElement.MessageVersion);
				return innerBindingElement.MessageVersion;
			}
            set {
				//Log.Debug(GetType().FullName + "::MessageVersion, " + value);
				innerBindingElement.MessageVersion = value;
			}
        }

        public override BindingElement Clone()
        {
            return new GZipMessageEncodingBindingElement(this.innerBindingElement);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            //Log.Debug(GetType().FullName + "::GetProperty<" + (typeof(T) != null ? typeof(T).GetType().FullName : null) + ">");
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return innerBindingElement.GetProperty<T>(context);
            }
            else 
            {
                return base.GetProperty<T>(context);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            //Log.Debug(GetType().FullName + "::BuildChannelFactory");
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            //Log.Debug(GetType().FullName + "::BuildChannelListener");
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            //Log.Debug(GetType().FullName + "::CanBuildChannelListener");
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }
}
