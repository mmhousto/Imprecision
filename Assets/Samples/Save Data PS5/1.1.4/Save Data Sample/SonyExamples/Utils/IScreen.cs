using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS5
public interface IScreen
{
	void Process(MenuStack stack);
	void OnEnter();
	void OnExit();
}
#endif
