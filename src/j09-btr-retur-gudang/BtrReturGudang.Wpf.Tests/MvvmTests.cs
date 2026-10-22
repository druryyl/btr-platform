using System.Threading.Tasks;
using BtrReturGudang.Wpf.Mvvm;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class MvvmTests
    {
        private class TestViewModel : ViewModelBase
        {
            private string _name;
            private int _count;

            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            public int Count
            {
                get => _count;
                set => SetProperty(ref _count, value);
            }

            public bool SetCountWithCallback(int newCount, System.Action callback)
            {
                return SetProperty(ref _count, newCount, callback);
            }
        }

        [Fact]
        public void ViewModelBase_SetProperty_RaisesPropertyChangedWhenValueChanges()
        {
            var vm = new TestViewModel();
            string changedProperty = null;
            vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            vm.Name = "New Name";

            Assert.Equal("New Name", vm.Name);
            Assert.Equal("Name", changedProperty);
        }

        [Fact]
        public void ViewModelBase_SetProperty_ReturnsFalseWhenValueIsSame()
        {
            var vm = new TestViewModel { Name = "Same" };
            int eventCount = 0;
            vm.PropertyChanged += (s, e) => eventCount++;

            vm.Name = "Same";

            Assert.Equal("Same", vm.Name);
            Assert.Equal(0, eventCount);
        }

        [Fact]
        public void ViewModelBase_SetProperty_InvokesCallbackOnlyOnChange()
        {
            var vm = new TestViewModel { Count = 5 };
            bool callbackInvoked = false;

            bool changed = vm.SetCountWithCallback(10, () => callbackInvoked = true);
            Assert.True(changed);
            Assert.True(callbackInvoked);

            callbackInvoked = false;
            changed = vm.SetCountWithCallback(10, () => callbackInvoked = true);
            Assert.False(changed);
            Assert.False(callbackInvoked);
        }

        [Fact]
        public void RelayCommand_ExecutesAndRespectsCanExecute()
        {
            bool executed = false;
            bool canExecute = true;

            var cmd = new RelayCommand(() => executed = true, () => canExecute);

            Assert.True(cmd.CanExecute(null));
            cmd.Execute(null);
            Assert.True(executed);

            canExecute = false;
            Assert.False(cmd.CanExecute(null));
        }

        [Fact]
        public void RelayCommandT_ExecutesWithParameter()
        {
            string receivedParam = null;
            var cmd = new RelayCommand<string>(param => receivedParam = param, p => p != null);

            Assert.False(cmd.CanExecute(null));
            Assert.True(cmd.CanExecute("test"));

            cmd.Execute("hello");
            Assert.Equal("hello", receivedParam);
        }

        [Fact]
        public async Task AsyncRelayCommand_TracksIsExecutingAndPreventsConcurrentExecution()
        {
            var tcs = new TaskCompletionSource<bool>();
            int executeCount = 0;

            var cmd = new AsyncRelayCommand(async () =>
            {
                executeCount++;
                await tcs.Task;
            });

            Assert.False(cmd.IsExecuting);
            Assert.True(cmd.CanExecute(null));

            var task = cmd.ExecuteAsync(null);

            Assert.True(cmd.IsExecuting);
            Assert.False(cmd.CanExecute(null));

            // Attempting second execution while first is running should be blocked
            await cmd.ExecuteAsync(null);
            Assert.Equal(1, executeCount);

            tcs.SetResult(true);
            await task;

            Assert.False(cmd.IsExecuting);
            Assert.True(cmd.CanExecute(null));
            Assert.Equal(1, executeCount);
        }
    }
}
