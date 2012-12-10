using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public enum Signal
    {
        RefreshMessage,
        RefreshDungeon,
        RefreshExpValue,
        SystemMessage,
        RefreshUser
    }

    // シグナルの付加情報も持たせるかな
    [Serializable()]
    public class SignalQueue : IEnumerable<Signal>
    {
        private String mSysteMessage;
        private List<Signal> mSignalList = new List<Signal>();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Signal> GetEnumerator()
        {
            return this.mSignalList.GetEnumerator();
        }

        public void addSignal(Signal signal_)
        {
            this.mSignalList.Add(signal_);
        }

        public void removeSignal(Signal signal_)
        {
            this.mSignalList.Remove(signal_);
        }
        public void clear()
        {
            this.mSignalList.Clear();
        }

        public String getSystemMessage()
        {
            return this.mSysteMessage;
        }
        public void setSystemMessage(String strMes_)
        {
            this.mSysteMessage = strMes_;
            this.mSignalList.Add(Signal.SystemMessage);
        }
    }
}


