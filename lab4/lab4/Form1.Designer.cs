namespace lab4
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.cmbAlgorithms = new System.Windows.Forms.ComboBox();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.btnDraw = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.numX0 = new System.Windows.Forms.NumericUpDown();
            this.numY0 = new System.Windows.Forms.NumericUpDown();
            this.numX1 = new System.Windows.Forms.NumericUpDown();
            this.numY1 = new System.Windows.Forms.NumericUpDown();
            this.numR = new System.Windows.Forms.NumericUpDown();
            this.btnRunBenchmark = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Y0 = new System.Windows.Forms.Label();
            this.labelX0 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Y1 = new System.Windows.Forms.Label();
            this.X1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelR = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numR)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCanvas.Location = new System.Drawing.Point(239, 12);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(711, 456);
            this.pnlCanvas.TabIndex = 0;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlCanvas_Paint);
            // 
            // cmbAlgorithms
            // 
            this.cmbAlgorithms.FormattingEnabled = true;
            this.cmbAlgorithms.Location = new System.Drawing.Point(30, 392);
            this.cmbAlgorithms.Name = "cmbAlgorithms";
            this.cmbAlgorithms.Size = new System.Drawing.Size(147, 24);
            this.cmbAlgorithms.TabIndex = 1;
            // 
            // numScale
            // 
            this.numScale.Location = new System.Drawing.Point(30, 98);
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(147, 22);
            this.numScale.TabIndex = 2;
            // 
            // btnDraw
            // 
            this.btnDraw.Location = new System.Drawing.Point(30, 187);
            this.btnDraw.Name = "btnDraw";
            this.btnDraw.Size = new System.Drawing.Size(147, 53);
            this.btnDraw.TabIndex = 4;
            this.btnDraw.Text = "Выполнить";
            this.btnDraw.UseVisualStyleBackColor = true;
            // 
            // lblTime
            // 
            this.lblTime.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(559, 483);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(38, 16);
            this.lblTime.TabIndex = 5;
            this.lblTime.Text = "00:00";
            // 
            // numX0
            // 
            this.numX0.Location = new System.Drawing.Point(84, 40);
            this.numX0.Name = "numX0";
            this.numX0.Size = new System.Drawing.Size(120, 22);
            this.numX0.TabIndex = 6;
            // 
            // numY0
            // 
            this.numY0.Location = new System.Drawing.Point(84, 87);
            this.numY0.Name = "numY0";
            this.numY0.Size = new System.Drawing.Size(120, 22);
            this.numY0.TabIndex = 7;
            // 
            // numX1
            // 
            this.numX1.Location = new System.Drawing.Point(84, 21);
            this.numX1.Name = "numX1";
            this.numX1.Size = new System.Drawing.Size(120, 22);
            this.numX1.TabIndex = 8;
            // 
            // numY1
            // 
            this.numY1.Location = new System.Drawing.Point(84, 61);
            this.numY1.Name = "numY1";
            this.numY1.Size = new System.Drawing.Size(120, 22);
            this.numY1.TabIndex = 9;
            // 
            // numR
            // 
            this.numR.Location = new System.Drawing.Point(84, 44);
            this.numR.Name = "numR";
            this.numR.Size = new System.Drawing.Size(120, 22);
            this.numR.TabIndex = 10;
            // 
            // btnRunBenchmark
            // 
            this.btnRunBenchmark.Location = new System.Drawing.Point(30, 288);
            this.btnRunBenchmark.Name = "btnRunBenchmark";
            this.btnRunBenchmark.Size = new System.Drawing.Size(147, 53);
            this.btnRunBenchmark.TabIndex = 11;
            this.btnRunBenchmark.Text = "Benchmark";
            this.btnRunBenchmark.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Y0);
            this.groupBox1.Controls.Add(this.labelX0);
            this.groupBox1.Controls.Add(this.numX0);
            this.groupBox1.Controls.Add(this.numY0);
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 129);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Параметры отрезка / центра";
            // 
            // Y0
            // 
            this.Y0.AutoSize = true;
            this.Y0.Location = new System.Drawing.Point(28, 87);
            this.Y0.Name = "Y0";
            this.Y0.Size = new System.Drawing.Size(23, 16);
            this.Y0.TabIndex = 9;
            this.Y0.Text = "Y0";
            // 
            // labelX0
            // 
            this.labelX0.AutoSize = true;
            this.labelX0.Location = new System.Drawing.Point(28, 40);
            this.labelX0.Name = "labelX0";
            this.labelX0.Size = new System.Drawing.Size(22, 16);
            this.labelX0.TabIndex = 8;
            this.labelX0.Text = "X0";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Y1);
            this.groupBox2.Controls.Add(this.X1);
            this.groupBox2.Controls.Add(this.numX1);
            this.groupBox2.Controls.Add(this.numY1);
            this.groupBox2.Location = new System.Drawing.Point(12, 199);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(221, 113);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Конечная точка отрезка";
            // 
            // Y1
            // 
            this.Y1.AutoSize = true;
            this.Y1.Location = new System.Drawing.Point(27, 63);
            this.Y1.Name = "Y1";
            this.Y1.Size = new System.Drawing.Size(23, 16);
            this.Y1.TabIndex = 11;
            this.Y1.Text = "Y1";
            // 
            // X1
            // 
            this.X1.AutoSize = true;
            this.X1.Location = new System.Drawing.Point(28, 23);
            this.X1.Name = "X1";
            this.X1.Size = new System.Drawing.Size(22, 16);
            this.X1.TabIndex = 10;
            this.X1.Text = "X1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelR);
            this.groupBox3.Controls.Add(this.numR);
            this.groupBox3.Location = new System.Drawing.Point(12, 368);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(221, 100);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Радиус (окружность / дуга)";
            // 
            // labelR
            // 
            this.labelR.AutoSize = true;
            this.labelR.Location = new System.Drawing.Point(0, 46);
            this.labelR.Name = "labelR";
            this.labelR.Size = new System.Drawing.Size(79, 16);
            this.labelR.TabIndex = 11;
            this.labelR.Text = "Радиус (R):";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.btnRunBenchmark);
            this.groupBox4.Controls.Add(this.numScale);
            this.groupBox4.Controls.Add(this.btnDraw);
            this.groupBox4.Controls.Add(this.cmbAlgorithms);
            this.groupBox4.Location = new System.Drawing.Point(975, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 453);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Управление";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 16);
            this.label6.TabIndex = 12;
            this.label6.Text = "Масштаб: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1209, 573);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pnlCanvas);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numR)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.ComboBox cmbAlgorithms;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Button btnDraw;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.NumericUpDown numX0;
        private System.Windows.Forms.NumericUpDown numY0;
        private System.Windows.Forms.NumericUpDown numX1;
        private System.Windows.Forms.NumericUpDown numY1;
        private System.Windows.Forms.NumericUpDown numR;
        private System.Windows.Forms.Button btnRunBenchmark;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelX0;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label Y0;
        private System.Windows.Forms.Label Y1;
        private System.Windows.Forms.Label X1;
        private System.Windows.Forms.Label labelR;
        private System.Windows.Forms.Label label6;
    }
}

