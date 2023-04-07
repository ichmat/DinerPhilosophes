using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinerPhilosophes
{
    internal class Philosophe 
    {
        private readonly Thread _instance;
        private int _num;
        private bool _already_eat;
        private int _max;
        private bool _cancel_pending = false;

        private PhilosopheState _state;

        public Philosophe(int num, int max)
        {
            _instance = new Thread(Act);
            _num = num;
            _already_eat = false;
            _max = max;
            _state = PhilosopheState.IsThinking;
        }

        public PhilosopheState GetPhilosopheState() { return _state; }

        public bool GetStateEat() => _already_eat;

        private void ChangePhilosopheState(PhilosopheState state)
        {
            _state = state;
            OnChangeState?.Invoke();
        }

        private void SetAlreadyEat()
        {
            if(!_already_eat )
            {
                _already_eat = true;
                OnFinishEat?.Invoke();
            }
        }

        public void Start()
        {
            _instance.Start();
        }

        public void Stop()
        {
            _cancel_pending = true;
        }

        private void Act()
        {
            Random rnd = new Random();
            int sec;
            while (!_cancel_pending)
            {
                ChangePhilosopheState(PhilosopheState.IsThinking);
                sec = rnd.Next(3, 12) + 1;
                do
                {
                    if (_cancel_pending) break;

                    sec--;
                    Thread.Sleep(1000);
                } while (sec > 0);

                if (_cancel_pending) break;

                ChangePhilosopheState(PhilosopheState.IsWaitingForEat);

                Program.forks.WaitForks(GetForkBefore(), GetForkAfter());

                ChangePhilosopheState(PhilosopheState.IsEating);

                sec = rnd.Next(3, 12) + 1;

                do
                {
                    if (_cancel_pending) break;

                    sec--;
                    Thread.Sleep(1000);
                } while (sec > 0);

                if (_cancel_pending)
                {
                    Program.forks.ReleaseForks(GetForkBefore(), GetForkAfter());
                    break;
                }

                Program.forks.ReleaseForks(GetForkBefore(), GetForkAfter());

                SetAlreadyEat();
            }

            ChangePhilosopheState(PhilosopheState.IsDEAD);
        }

        private int GetForkBefore()
        {
            return _num;
        }

        private int GetForkAfter()
        {
            if( _num == _max - 1) return 0;
            else return _num+1;
        }

        public delegate void FinishEat();
        public event FinishEat OnFinishEat;

        public delegate void ChangeState();
        public event ChangeState OnChangeState;
    }

    internal enum PhilosopheState
    {
        IsThinking = 0,
        IsWaitingForEat = 1,
        IsEating = 2,
        IsDEAD = 3,
    }
}
