using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;
using NVelocity.Exception;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Templates
{
    public class TemplateManager : ITemplateManager 
    {
        //private VelocityEngine vEngine = null;

        public TemplateManager()
        {
            
            Velocity.Init(); 
            //vEngine = new VelocityEngine();
            //vEngine.SetProperty("assembly.resource.loader.cache", "true");
            //vEngine.Init();
        }
    
        #region ITemplateManager Members

        private Dictionary<string, ITemplate> _Templates;
        public Dictionary<string,ITemplate>  Templates
        {
	        get 
            { 
                if (_Templates == null)
                    _Templates = new Dictionary<string,ITemplate>();
                return _Templates;
            }
        }

        public ITemplate CreateTemplate(string name, string Body)
        {
 	        ITemplate template = Injector.Get<ITemplate>();
            template.Name = name;
            template.Body = Body;
            return template;
        }

        public ITemplate CreateTemplateFromFile(string name, string fileName)
        {
 	        ITemplate template = Injector.Get<ITemplate>();
            StreamReader reader = File.OpenText(fileName);
            template.Name = name;
            template.Body = reader.ReadToEnd(); 
            reader.Close();
            return template;
        }

        public string ProcessTemplate(ITemplate template)
        {
            return ProcessTemplate(template.Body, template.Context);
        }

        public string  ProcessTemplate(string templateValue, Hashtable context)
        {
 	        VelocityContext vContext = new VelocityContext(context);
            
            //foreach(string key in context.Keys)
            //    vContext.Put(key, context[key]);
 
            string result = null;

            if (!string.IsNullOrEmpty(templateValue))
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                try
                {
                    if (Velocity.Evaluate(vContext, writer, "", templateValue))
                        result = writer.ToString();
                }
                catch (ParseErrorException pe)
                {
                    return pe.Message;
                }
                catch (MethodInvocationException mi)
                {
                    return mi.Message;
                }
                
            }
            return result;
        }

        public string ProcessTemplateFromFile(string fileName, Hashtable context)
        {
            ITemplate template = CreateTemplateFromFile(fileName, fileName);
            template.Context = context;
            return ProcessTemplate(template);
        }

        #endregion
    }
}
