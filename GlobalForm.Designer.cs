namespace Verde
{
    partial class GlobalForm
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalForm));
            this.InnerBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // InnerBrowser
            // 
            this.InnerBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InnerBrowser.Location = new System.Drawing.Point(0, 0);
            this.InnerBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.InnerBrowser.Name = "InnerBrowser";
            this.InnerBrowser.Size = new System.Drawing.Size(472, 766);
            this.InnerBrowser.TabIndex = 0;
            this.InnerBrowser.Url = new System.Uri("http://pya.cc/ipn/index.php", System.UriKind.Absolute);
            // 
            // GlobalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 766);
            this.Controls.Add(this.InnerBrowser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GlobalForm";
            this.Text = "Verde";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser InnerBrowser;
    }
}

