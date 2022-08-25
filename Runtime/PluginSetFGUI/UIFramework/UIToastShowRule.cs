using System.Collections.Generic;
#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace PluginSet.FGUI
{
    public abstract class UIToastShowRule
    {
        public static readonly UIToastShowRule DefaultRule = new UIToastShowRuleDefault(3f, true, true);
        
        private static readonly Dictionary<string, UIToastShowRule> _rules = new Dictionary<string, UIToastShowRule>();
        
        public static UIToastShowRule GetRule(string name)
        {
            if (_rules.TryGetValue(name, out var result))
                return result;

            return DefaultRule;
        }

        public static void AddRule(string name, UIToastShowRule rule)
        {
            if (_rules.ContainsKey(name))
                _rules[name] = rule;
            else
                _rules.Add(name, rule);
        }

#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public abstract void OnToastAdded(UIToast toast);

#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public abstract void OnToastOut(UIToast toast);

#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public abstract void OnToastRemove(UIToast toast);
    }
}