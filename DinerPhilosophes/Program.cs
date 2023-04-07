using System.Text;

namespace DinerPhilosophes
{
    internal class Program
    {
        internal static Forks forks;
        private static Philosophe[] philosophes;
        private static bool _close_pending = false;
        private static ManualResetEvent resetEvent = new ManualResetEvent(false);
        private static bool _able_write_console = false;


        static void Main(string[] args)
        {
            Console.WriteLine("start");

            resetEvent.Reset();
            int nbPhilosophes = 15;
            forks = new Forks(nbPhilosophes);
            philosophes = new Philosophe[nbPhilosophes];

            for(int i = 0; i < philosophes.Length; i++)
            {
                philosophes[i] = new Philosophe(i, nbPhilosophes);
                philosophes[i].OnChangeState += Program_OnChangeState;
                philosophes[i].OnFinishEat += Program_OnFinishEat;
            }

            StartAll();
            _able_write_console = true;
            resetEvent.WaitOne();

            Console.WriteLine("finish");
        }

        private static void StartAll()
        {
            Array.ForEach(philosophes, (p) => p.Start());
        }

        private static void StopAll()
        {
            Array.ForEach(philosophes, (p) => p.Stop());
            _close_pending = true;
        }

        private static void Program_OnFinishEat()
        {
            for(int i = 0;i < philosophes.Length; i++)
            {
                if (!philosophes[i].GetStateEat())
                {
                    return;
                }
            }

            StopAll();
        }

        private static SemaphoreSlim _write = new SemaphoreSlim(1);

        private static void Program_OnChangeState()
        {
            if (!_able_write_console) return;

            _write.Wait();

            StringBuilder sb = new StringBuilder();
            bool alldead = true;
            for(int i = 0; i < philosophes.Length; i++)
            {
                sb.Append("philosophe " + i.ToString());
                switch (philosophes[i].GetPhilosopheState())
                {
                    case PhilosopheState.IsThinking:
                        sb.Append(" mmmmmmmmmmmmmmmmmh..."); alldead = false; break;
                    case PhilosopheState.IsWaitingForEat:
                        sb.Append(" attend de manger"); alldead = false; break;
                    case PhilosopheState.IsEating:
                        sb.Append(" mange"); alldead = false; break;
                    case PhilosopheState.IsDEAD:
                        sb.Append(" est mort"); break;
                }
                sb.Append('\n');
            }
            //Console.Clear();
            Console.WriteLine(sb.ToString());

            _write.Release();

            if (_close_pending && alldead)
            {
                resetEvent.Set();
            }
        }
    }
}