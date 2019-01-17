using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace MtHubPlugin
{
    public partial class MtHubOptionsForm : Form
    {
        public MtHubOptionsForm()
        {
            InitializeComponent();
            LocalizeControls();
        }

        public MtHubOptions Options { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Options.SecureSettings.ApiToken == string.Empty)
                return;

            tbApiToken.Text = Options.SecureSettings.ApiToken;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbApiToken.Text != string.Empty)
                Options.SecureSettings.ApiToken = tbApiToken.Text;
            else
                ShowMissingTokenError();
        }

        private void ShowMissingTokenError()
        {
            lblValidateToken.Text = LocalizationHelper.Instance.GetResourceString("MissingTokenErrorText");
            lblValidateToken.ForeColor = Color.Red;
        }

        private void tbApiToken_TextChanged(object sender, EventArgs e)
        {
            if (tbApiToken.Text == string.Empty)
            {
                btnOK.Enabled = false;

                ShowMissingTokenError();
            }
            else
            {
                btnOK.Enabled = true;
                lblValidateToken.Text = string.Empty;
            }
            
        }

        private void LocalizeControls()
        {
            Text = LocalizationHelper.Instance.GetResourceString("OptionsForm.Text");
            lblApiToken.Text = LocalizationHelper.Instance.GetResourceString("OptionsForm.ApiTokenLabel");
            btnOK.Text = LocalizationHelper.Instance.GetResourceString("OptionsForm.OKButton");
        }

        private void tlpMain_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}