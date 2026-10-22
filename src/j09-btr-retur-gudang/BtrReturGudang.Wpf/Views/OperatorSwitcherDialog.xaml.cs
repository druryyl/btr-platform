using System;
using System.Text.RegularExpressions;
using System.Windows;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Wpf.Views
{
    /// <summary>
    /// Interaction logic for OperatorSwitcherDialog.xaml.
    /// Modal dialog for switching active shift worker identity (display name + registered Google email).
    /// Persists operator name and Google email to workstation registry via IRegistryHelper.
    /// Traceable to: ARCHITECTURE §5.2 SCR-01, §5.2 SCR-07, TD-007, TD-012, P4-S10.
    /// </summary>
    public partial class OperatorSwitcherDialog : Window
    {
        private static readonly Regex EmailShapeRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly IRegistryHelper _registryHelper;

        public string SelectedOperatorName { get; private set; }
        public string SelectedOperatorEmail { get; private set; }

        public OperatorSwitcherDialog() : this(null)
        {
        }

        public OperatorSwitcherDialog(IRegistryHelper registryHelper)
        {
            InitializeComponent();
            _registryHelper = registryHelper;

            string currentOp = _registryHelper?.LastOperatorName ?? string.Empty;
            string currentEmail = _registryHelper?.LastOperatorEmail ?? string.Empty;

            TxtOperatorName.Text = currentOp;
            TxtOperatorEmail.Text = currentEmail;

            LblHint.Text = string.IsNullOrWhiteSpace(currentOp)
                ? "Belum ada operator tersimpan di registry workstation ini."
                : $"Operator aktif sebelumnya: \"{currentOp}\" ({currentEmail})";
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            string name = TxtOperatorName.Text?.Trim();
            string email = TxtOperatorEmail.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Nama operator tidak boleh kosong.", "Validasi Operator", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtOperatorName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Google email operator tidak boleh kosong.", "Validasi Operator", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtOperatorEmail.Focus();
                return;
            }

            if (!EmailShapeRegex.IsMatch(email))
            {
                MessageBox.Show("Format Google email operator tidak valid (contoh: nama@btr.co.id).", "Validasi Operator", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtOperatorEmail.Focus();
                return;
            }

            if (_registryHelper != null)
            {
                _registryHelper.LastOperatorName = name;
                _registryHelper.LastOperatorEmail = email;
            }

            SelectedOperatorName = name;
            SelectedOperatorEmail = email;

            try
            {
                DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                // In headless unit tests, window is not shown with ShowDialog()
            }
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch (InvalidOperationException)
            {
            }
            Close();
        }
    }
}
