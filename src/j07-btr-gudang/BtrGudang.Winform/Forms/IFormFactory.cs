using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace BtrGudang.Winform
{
    namespace BtrGudang.Winform.Services
    {
        public interface IFormFactory
        {
            T CreateForm<T>() where T : Form;
        }

        public class FormFactory : IFormFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public FormFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public T CreateForm<T>() where T : Form
            {
                return _serviceProvider.GetRequiredService<T>();
            }
        }
    }
}
