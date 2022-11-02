using System.Collections.Generic;
using System.Xml;
using RimWorld.Planet;
using Verse;
using static HarmonyLib.AccessTools;

namespace ThreadSafeLinkedList
{
    public static class Scribe_Collections_Patch
    {
        public static FieldRef<CrossRefHandler, LoadedObjectDirectory> loadedObjectDirectoryRef = FieldRefAccess<CrossRefHandler, LoadedObjectDirectory>("loadedObjectDirectory");

        public static void Look1<T>(ref ThreadSafeLinkedList<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            Look(ref list, saveDestroyedThings: false, label, lookMode, ctorArgs);
        }

        public static void Look2<T>(ref ThreadSafeLinkedList<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            Look(ref list, saveDestroyedThings: false, label, lookMode, ctorArgs);
        }

        public static void Look<T>(ref ThreadSafeLinkedList<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            Look2(ref list, saveDestroyedThings: false, label, lookMode, ctorArgs);
        }

        public static void Look<T>(ref ThreadSafeLinkedList<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
        {
            if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
            {
                Log.Error(string.Concat("LookList call with a list of ", typeof(T), " must have lookMode set explicitly."));
                return;
            }
            if (Scribe.EnterNode(label))
            {
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        if (list != null)
                        {
                            foreach (T item in list)
                            {
                                switch (lookMode)
                                {
                                    case LookMode.Value:
                                        {
                                            T value5 = item;
                                            Scribe_Values.Look(ref value5, "li", default(T), forceSave: true);
                                            break;
                                        }
                                    case LookMode.LocalTargetInfo:
                                        {
                                            LocalTargetInfo value4 = (LocalTargetInfo)(object)item;
                                            Scribe_TargetInfo.Look(ref value4, saveDestroyedThings, "li");
                                            break;
                                        }
                                    case LookMode.TargetInfo:
                                        {
                                            TargetInfo value3 = (TargetInfo)(object)item;
                                            Scribe_TargetInfo.Look(ref value3, saveDestroyedThings, "li");
                                            break;
                                        }
                                    case LookMode.GlobalTargetInfo:
                                        {
                                            GlobalTargetInfo value2 = (GlobalTargetInfo)(object)item;
                                            Scribe_TargetInfo.Look(ref value2, saveDestroyedThings, "li");
                                            break;
                                        }
                                    case LookMode.Def:
                                        {
                                            Def value = (Def)(object)item;
                                            Scribe_Defs.Look(ref value, "li");
                                            break;
                                        }
                                    case LookMode.BodyPart:
                                        {
                                            BodyPartRecord part = (BodyPartRecord)(object)item;
                                            Scribe_BodyParts.Look(ref part, "li");
                                            break;
                                        }
                                    case LookMode.Deep:
                                        {
                                            T target = item;
                                            Scribe_Deep.Look(ref target, saveDestroyedThings, "li", ctorArgs);
                                            break;
                                        }
                                    case LookMode.Reference:
                                        {
                                            ILoadReferenceable refee = (ILoadReferenceable)(object)item;
                                            Scribe_References.Look(ref refee, "li", saveDestroyedThings);
                                            break;
                                        }
                                }
                            }
                            return;
                        }
                        Scribe.saver.WriteAttribute("IsNull", "True");
                    }
                    else if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        XmlNode curXmlParent = Scribe.loader.curXmlParent;
                        XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
                        if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
                        {
                            if (lookMode == LookMode.Reference)
                            {
                                Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
                            }
                            list = null;
                        }
                        else
                        {
                            switch (lookMode)
                            {
                                case LookMode.Value:
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode in curXmlParent.ChildNodes)
                                        {
                                            T val2 = ScribeExtractor.ValueFromNode(childNode, default(T));
                                            list.Add(val2);
                                        }
                                        break;
                                    }
                                case LookMode.Deep:
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode2 in curXmlParent.ChildNodes)
                                        {
                                            T val8 = ScribeExtractor.SaveableFromNode<T>(childNode2, ctorArgs);
                                            list.Add(val8);
                                        }
                                        break;
                                    }
                                case LookMode.Def:
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    {
                                        foreach (XmlNode childNode3 in curXmlParent.ChildNodes)
                                        {
                                            T val7 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode3);
                                            list.Add(val7);
                                        }
                                        break;
                                    }
                                case LookMode.BodyPart:
                                    {
                                        list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                        int num4 = 0;
                                        {
                                            foreach (XmlNode childNode4 in curXmlParent.ChildNodes)
                                            {
                                                T val6 = (T)(object)ScribeExtractor.BodyPartFromNode(childNode4, num4.ToString(), null);
                                                list.Add(val6);
                                                num4++;
                                            }
                                            break;
                                        }
                                    }
                                case LookMode.LocalTargetInfo:
                                    {
                                        list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                        int num3 = 0;
                                        {
                                            foreach (XmlNode childNode5 in curXmlParent.ChildNodes)
                                            {
                                                T val5 = (T)(object)ScribeExtractor.LocalTargetInfoFromNode(childNode5, num3.ToString(), LocalTargetInfo.Invalid);
                                                list.Add(val5);
                                                num3++;
                                            }
                                            break;
                                        }
                                    }
                                case LookMode.TargetInfo:
                                    {
                                        list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                        int num2 = 0;
                                        {
                                            foreach (XmlNode childNode6 in curXmlParent.ChildNodes)
                                            {
                                                T val4 = (T)(object)ScribeExtractor.TargetInfoFromNode(childNode6, num2.ToString(), TargetInfo.Invalid);
                                                list.Add(val4);
                                                num2++;
                                            }
                                            break;
                                        }
                                    }
                                case LookMode.GlobalTargetInfo:
                                    {
                                        list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                        int num = 0;
                                        {
                                            foreach (XmlNode childNode7 in curXmlParent.ChildNodes)
                                            {
                                                T val3 = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode(childNode7, num.ToString(), GlobalTargetInfo.Invalid);
                                                list.Add(val3);
                                                num++;
                                            }
                                            break;
                                        }
                                    }
                                case LookMode.Reference:
                                    {
                                        List<string> list2 = new List<string>(curXmlParent.ChildNodes.Count);
                                        foreach (XmlNode childNode8 in curXmlParent.ChildNodes)
                                        {
                                            list2.Add(childNode8.InnerText);
                                        }
                                        Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(list2, "");
                                        break;
                                    }
                            }
                        }
                    }
                    else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                    {
                        switch (lookMode)
                        {
                            case LookMode.Reference:
                                list = TakeResolvedRefList<T>(Scribe.loader.crossRefs, "");
                                break;
                            case LookMode.LocalTargetInfo:
                                if (list != null)
                                {
                                    int num6 = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> val10 = list.firstNode;
                                        while (val10 != null)
                                        {
                                            val10.value = (T)(object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)(object)val10.value, num6.ToString());
                                            val10 = val10.nextNode;
                                            num6++;
                                        }
                                        break;
                                    }
                                }
                                break;
                            case LookMode.TargetInfo:
                                if (list != null)
                                {
                                    int num7 = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> val11 = list.firstNode;
                                        while (val11 != null)
                                        {
                                            val11.value = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)val11.value, num7.ToString());
                                            val11 = val11.nextNode;
                                            num7++;
                                        }
                                        break;
                                    }
                                }
                                break;
                            case LookMode.GlobalTargetInfo:
                                if (list != null)
                                {
                                    int num5 = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> val9 = list.firstNode;
                                        while (val9 != null)
                                        {
                                            val9.value = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)(object)val9.value, num5.ToString());
                                            val9 = val9.nextNode;
                                            num5++;
                                        }
                                        break;
                                    }
                                }
                                break;
                            case LookMode.Def:
                                break;
                        }
                    }
                    return;
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (lookMode == LookMode.Reference)
                {
                    Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
                }
                list = null;
            }
        }

        public static ThreadSafeLinkedList<T> TakeResolvedRefList<T>(CrossRefHandler __instance, string toAppendToPathRelToParent)
        {
            string text = Scribe.loader.curPathRelToParent;
            if (!toAppendToPathRelToParent.NullOrEmpty())
            {
                text = text + "/" + toAppendToPathRelToParent;
            }
            return TakeResolvedRefList<T>(__instance, text, Scribe.loader.curParent);
        }

        public static ThreadSafeLinkedList<T> TakeResolvedRefList<T>(CrossRefHandler __instance, string pathRelToParent, IExposable parent)
        {
            List<string> list = __instance.loadIDs.TakeList(pathRelToParent, parent);
            ThreadSafeLinkedList<T> val = new ThreadSafeLinkedList<T>();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    val.Add(loadedObjectDirectoryRef.Invoke(__instance).ObjectWithLoadID<T>(list[i]));
                }
            }
            return val;
        }
    }
}