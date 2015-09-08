using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    internal class AppEventManager : IAppEventManager
    {
        #region private members

        private Dictionary<Type, List<Delegate>> _events = new Dictionary<Type, List<Delegate>>();
        private Hashtable _modules = new Hashtable();

        #endregion

        public AppEventManager(XmlNode applicationEvents)
        {
            Load(applicationEvents);
        }

        /// <summary>
        ///   Reads the event manager configuration.
        /// </summary>
        private void Load(XmlNode node)
        {
            if (node != null)
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.NodeType != XmlNodeType.Comment)
                    {
                        try
                        {
                            switch (n.Name)
                            {
                                case "clear":
                                    _modules.Clear();
                                    break;
                                case "remove":
                                    RemoveEventModule(n);

                                    break;
                                case "add":
                                    AddEventModule(n);
                                    break;
                            }
                        }
                        catch (Exception E)
                        {
                            throw new SettingsException(String.Format("Error in configuration for module '{0}', action '{1}', type '{2}'", n.Attributes["name"], n.Attributes["type"], n.Name), E);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   Removes the event module.
        /// </summary>
        /// <param name = "n">The n.</param>
        private void RemoveEventModule(XmlNode n)
        {
            XmlAttribute removeNameAtt = n.Attributes["name"];
            string removeName = removeNameAtt == null ? null : removeNameAtt.Value;

            if (!string.IsNullOrEmpty(removeName) && _modules.ContainsKey(removeName))
            {
                _modules.Remove(removeName);
            }
        }


        /// <summary>
        ///   Adds the event module.
        /// </summary>
        /// <param name = "n">The n.</param>
        private void AddEventModule(XmlNode n)
        {
            XmlAttribute en = n.Attributes["enabled"];

            if (en != null && en.Value == "false")
                return;

            XmlAttribute nameAtt = n.Attributes["name"];
            XmlAttribute typeAtt = n.Attributes["type"];
            string name = nameAtt == null ? null : nameAtt.Value;
            string itype = typeAtt == null ? null : typeAtt.Value;

            if (string.IsNullOrEmpty(name))
            {
                throw new SettingsException((string.Format("A IAppEventModule could not be loaded. The name was not defined. Type {0}", itype)));
            }

            if (string.IsNullOrEmpty(itype))
            {
                throw new SettingsException((string.Format("A IAppEventModule ({0}) could not be loaded. No type was defined", name)));
            }

            Type type = Type.GetType(itype);

            if (type == null)
            {
                throw new SettingsException((string.Format("A IAppEventModule ({0}) could not be loaded. The type {1} does not exist", name, itype)));
            }

            IAppEventModule mod = Activator.CreateInstance(type) as IAppEventModule;

            if (mod == null)
            {
                throw new SettingsException((string.Format("A IAppEventModule ({0}) could not be loaded. The type {1} could not be instantiated", name, itype)));
            }

            mod.Initialize(this, n);
            _modules.Add(name, mod);
        }

        #region Execute Events

        /// <summary>
        /// Executes the event.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        /// <param name="state">The state.</param>
        /// <param name="eventItem">The event item.</param>
        public void ExecuteEvent(AppEventType appType, string state, object eventItem)
        {
            Type[] tArray = eventItem.GetType().FindInterfaces(Module.FilterTypeName, "*");

            foreach(Type type in tArray)
            {
                ExecuteEventImpl(type, appType, state, eventItem);
            }

            ExecuteEventImpl(eventItem.GetType(), appType, state, eventItem);
        }

        /// <summary>
        /// Executes the event.
        /// </summary>
        /// <param name="eventActionType">Type of the event action.</param>
        /// <param name="type">The type.</param>
        /// <param name="state">The state.</param>
        /// <param name="eventItem">The event item.</param>
        protected void ExecuteEventImpl(Type eventActionType, AppEventType type, string state, object eventItem)
        {
            List<Delegate> delegateList;

            if (_events.TryGetValue(eventActionType, out delegateList))
            {
                foreach (Delegate delgate in delegateList)
                {
                    try
                    {
                        delgate.DynamicInvoke(eventItem, new AppEventArgs(type, state));
                    }
                    catch (Exception ex)
                    {
                        if (ExceptionHandler != null)
                        {
                            ExceptionHandler(ex);
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///   Adds the shop event.
        /// </summary>
        /// <param name = "objectType">Type of the object.</param>
        /// <param name = "addEvent">The add event.</param>
        public void RegisterHandler(Type objectType, AppEventHandler addEvent)
        {
            if (!_events.ContainsKey(objectType))
            {
                _events[objectType] = new List<Delegate>();
            }

            _events[objectType].Add(addEvent);
        }

        public event AppExceptionHandler ExceptionHandler;

        #endregion
    }
}