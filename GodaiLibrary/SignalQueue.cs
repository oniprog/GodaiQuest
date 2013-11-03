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

        public SignalQueue() { }

        public SignalQueue(godaiquest.SignalQueue signalqueue)
        {
            mSysteMessage = signalqueue.system_message;
			foreach (var tmp in signalqueue.signals)
			{
                mSignalList.Add((Signal)tmp.signal);
			}
        }

        public godaiquest.SignalQueue getSerialize()
        {
            var ret = new godaiquest.SignalQueue();
            ret.system_message = mSysteMessage;
			foreach ( var tmp in mSignalList)
			{
                var signal = new godaiquest.Signal();
                signal.signal = (int)tmp;
                ret.signals.Add(signal);
			}
            return ret;
        }

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


