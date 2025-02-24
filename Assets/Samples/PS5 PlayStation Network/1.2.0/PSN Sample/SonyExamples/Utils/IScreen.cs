using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSNSample
{
    public interface IScreen
    {
#if UNITY_PS5 || UNITY_PS4
        void Process(MenuStack stack);
        void OnEnter();
        void OnExit();
#endif
    }
}
