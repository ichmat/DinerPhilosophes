using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinerPhilosophes
{
    internal class Forks
    {
        private readonly SemaphoreSlim[] _forks;
        private readonly SemaphoreSlim _check_fork = new SemaphoreSlim(1);

        public Forks(int num)
        {
            _forks = new SemaphoreSlim[num];
            for (int i = 0; i < num; i++)
            {
                _forks[i] = new SemaphoreSlim(1);
            }
        }

        internal void WaitForks(int numFork1, int numFork2)
        {
            bool forkGetted = false;
            do
            {
                _check_fork.Wait();

                if(_forks[numFork1].CurrentCount > 0 && _forks[numFork2].CurrentCount > 0)
                {
                    _forks[numFork1].Wait();
                    _forks[numFork2].Wait();
                    forkGetted = true;
                }

                _check_fork.Release();

                if(!forkGetted)
                    Thread.Sleep(50);
            } 
            while (!forkGetted);
        }

        internal void ReleaseForks(int numFork1, int numFork2)
        {
            _check_fork.Wait();

            _forks[numFork1].Release();
            _forks[numFork2].Release();

            _check_fork.Release();
        }
    }
}
