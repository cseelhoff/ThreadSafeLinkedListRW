using RimWorld.Planet;
using System.Collections.Generic;
using System.Xml;
using Verse;
using static HarmonyLib.AccessTools;

namespace ThreadSafeLinkedList
{
    public static class Scribe_Collections_Patch
    {
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
            }
            else if (Scribe.EnterNode(label))
            {
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving)
                    {
                        if (list == null)
                        {
                            Scribe.saver.WriteAttribute("IsNull", "True");
                            return;
                        }
                        foreach (T item8 in list)
                        {
                            switch (lookMode)
                            {
                                case LookMode.Value:
                                    {
                                        T value5 = item8;
                                        Scribe_Values.Look(ref value5, "li", default(T), forceSave: true);
                                        break;
                                    }
                                case LookMode.LocalTargetInfo:
                                    {
                                        LocalTargetInfo value4 = (LocalTargetInfo)(object)item8;
                                        Scribe_TargetInfo.Look(ref value4, saveDestroyedThings, "li");
                                        break;
                                    }
                                case LookMode.TargetInfo:
                                    {
                                        TargetInfo value3 = (TargetInfo)(object)item8;
                                        Scribe_TargetInfo.Look(ref value3, saveDestroyedThings, "li");
                                        break;
                                    }
                                case LookMode.GlobalTargetInfo:
                                    {
                                        GlobalTargetInfo value2 = (GlobalTargetInfo)(object)item8;
                                        Scribe_TargetInfo.Look(ref value2, saveDestroyedThings, "li");
                                        break;
                                    }
                                case LookMode.Def:
                                    {
                                        Def value = (Def)(object)item8;
                                        Scribe_Defs.Look(ref value, "li");
                                        break;
                                    }
                                case LookMode.BodyPart:
                                    {
                                        BodyPartRecord part = (BodyPartRecord)(object)item8;
                                        Scribe_BodyParts.Look(ref part, "li");
                                        break;
                                    }
                                case LookMode.Deep:
                                    {
                                        T target = item8;
                                        Scribe_Deep.Look(ref target, saveDestroyedThings, "li", ctorArgs);
                                        break;
                                    }
                                case LookMode.Reference:
                                    {
                                        ILoadReferenceable refee = (ILoadReferenceable)(object)item8;
                                        Scribe_References.Look(ref refee, "li", saveDestroyedThings);
                                        break;
                                    }
                            }
                        }
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
                            return;
                        }
                        switch (lookMode)
                        {
                            case LookMode.Value:
                                list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                foreach (XmlNode childNode in curXmlParent.ChildNodes)
                                {
                                    T item = ScribeExtractor.ValueFromNode(childNode, default(T));
                                    list.Add(item);
                                }
                                break;
                            case LookMode.Deep:
                                list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                foreach (XmlNode childNode2 in curXmlParent.ChildNodes)
                                {
                                    T item7 = ScribeExtractor.SaveableFromNode<T>(childNode2, ctorArgs);
                                    list.Add(item7);
                                }
                                break;
                            case LookMode.Def:
                                list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                foreach (XmlNode childNode3 in curXmlParent.ChildNodes)
                                {
                                    T item6 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode3);
                                    list.Add(item6);
                                }
                                break;
                            case LookMode.BodyPart:
                                {
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    int num4 = 0;
                                    foreach (XmlNode childNode4 in curXmlParent.ChildNodes)
                                    {
                                        T item5 = (T)(object)ScribeExtractor.BodyPartFromNode(childNode4, num4.ToString(), null);
                                        list.Add(item5);
                                        num4++;
                                    }
                                    break;
                                }
                            case LookMode.LocalTargetInfo:
                                {
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    int num3 = 0;
                                    foreach (XmlNode childNode5 in curXmlParent.ChildNodes)
                                    {
                                        T item4 = (T)(object)ScribeExtractor.LocalTargetInfoFromNode(childNode5, num3.ToString(), LocalTargetInfo.Invalid);
                                        list.Add(item4);
                                        num3++;
                                    }
                                    break;
                                }
                            case LookMode.TargetInfo:
                                {
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    int num2 = 0;
                                    foreach (XmlNode childNode6 in curXmlParent.ChildNodes)
                                    {
                                        T item3 = (T)(object)ScribeExtractor.TargetInfoFromNode(childNode6, num2.ToString(), TargetInfo.Invalid);
                                        list.Add(item3);
                                        num2++;
                                    }
                                    break;
                                }
                            case LookMode.GlobalTargetInfo:
                                {
                                    list = new ThreadSafeLinkedList<T>(curXmlParent.ChildNodes.Count);
                                    int num = 0;
                                    foreach (XmlNode childNode7 in curXmlParent.ChildNodes)
                                    {
                                        T item2 = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode(childNode7, num.ToString(), GlobalTargetInfo.Invalid);
                                        list.Add(item2);
                                        num++;
                                    }
                                    break;
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
                    else
                    {
                        if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
                        {
                            return;
                        }
                        switch (lookMode)
                        {
                            case LookMode.Reference:
                                list = TakeResolvedRefList<T>(Scribe.loader.crossRefs, "");
                                break;
                            case LookMode.LocalTargetInfo:
                                if (list != null)
                                {
                                    //for (int j = 0; j < list.Count; j++)
                                    //{
                                    //    list[j] = (T)(object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)(object)list[j], j.ToString());
                                    //}
                                    int j = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> currentNode = list.firstNode;
                                        while(currentNode != null)
                                        {
                                            currentNode.value = (T)(object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)(object)currentNode.value, j.ToString());
                                            currentNode = currentNode.nextNode;
                                            j++;
                                        }
                                    }
                                }
                                break;
                            case LookMode.TargetInfo:
                                if (list != null)
                                {
                                    //for (int k = 0; k < list.Count; k++)
                                    //{
                                    //    list[k] = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)list[k], k.ToString());
                                    //}
                                    int k = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> currentNode = list.firstNode;
                                        while (currentNode != null)
                                        {
                                            currentNode.value = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)currentNode.value, k.ToString());
                                            currentNode = currentNode.nextNode;
                                            k++;
                                        }
                                    }
                                }
                                break;
                            case LookMode.GlobalTargetInfo:
                                if (list != null)
                                {
                                    //for (int i = 0; i < list.Count; i++)
                                    //{
                                    //    list[i] = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)(object)list[i], i.ToString());
                                    //}
                                    int i = 0;
                                    lock (list)
                                    {
                                        ThreadSafeNode<T> currentNode = list.firstNode;
                                        while (currentNode != null)
                                        {
                                            currentNode.value = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)(object)currentNode.value, i.ToString());
                                            currentNode = currentNode.nextNode;
                                            i++;
                                        }
                                    }
                                }
                                break;
                        }
                        return;
                    }
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
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

        public static FieldRef<CrossRefHandler, LoadedObjectDirectory> loadedObjectDirectoryRef = FieldRefAccess<CrossRefHandler, LoadedObjectDirectory>("loadedObjectDirectory");

        public static ThreadSafeLinkedList<T> TakeResolvedRefList<T>(CrossRefHandler __instance, string pathRelToParent, IExposable parent)
        {
            List<string> list = __instance.loadIDs.TakeList(pathRelToParent, parent);
            ThreadSafeLinkedList<T> list2 = new ThreadSafeLinkedList<T>();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list2.Add(loadedObjectDirectoryRef(__instance).ObjectWithLoadID<T>(list[i]));
                }
            }
            return list2;
        }

    }
}
