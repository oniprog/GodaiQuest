using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public enum SignalType
    {
        RefreshMessage,
        RefreshDungeon,
        RefreshExpValue,
        SystemMessage,
        RefreshUser,
		DestroyMonster
    }

    public class Signal
    {
        public SignalType SigType { get; set; }
        public int ID { get; set; }
		public int Ix { get; set; }
		public int Iy { get; set; }

        public Signal(SignalType sig)
        {
            SigType = sig;
        }
        public Signal(godaiquest.Signal sig)
        {
            SigType = (SignalType) sig.signal;
            ID = sig.id;
            Ix = sig.ix;
            Iy = sig.iy;
        }

        public godaiquest.Signal getSerialize()
        {
            var sig = new godaiquest.Signal();
            sig.id = ID;
            sig.signal = (int) SigType;
            sig.ix = Ix;
            sig.iy = Iy;
            return sig;
        }
    }

    // シグナルの付加情報も持たせるかな
    [Serializable()]
    public class SignalQueue : IEnumerable<Signal>
    {
        private String mSysteMessage;
        private List<Signal> _SignalList = new List<Signal>();

        public SignalQueue() { }

        public SignalQueue(godaiquest.SignalQueue signalqueue)
        {
            mSysteMessage = signalqueue.system_message;
			foreach (var tmp in signalqueue.signals)
			{
			    Signal sig = new Signal(tmp);
                _SignalList.Add(sig);
			}
        }

        public godaiquest.SignalQueue getSerialize()
        {
            var ret = new godaiquest.SignalQueue();
            ret.system_message = mSysteMessage;
			foreach ( var tmp in _SignalList)
			{
			    ret.signals.Add(tmp.getSerialize());
			}
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Signal> GetEnumerator()
        {
            return this._SignalList.GetEnumerator();
        }

        public void addSignal(Signal signal_)
        {
            this._SignalList.Add(signal_);
        }

        public void removeSignal(Signal signal_)
        {
            this._SignalList.Remove(signal_);
        }
        public void clear()
        {
            this._SignalList.Clear();
        }

        public String getSystemMessage()
        {
            return this.mSysteMessage;
        }
        public void setSystemMessage(String strMes_)
        {
            this.mSysteMessage = strMes_;
            this._SignalList.Add(new Signal(SignalType.SystemMessage));
        }
    }
}


