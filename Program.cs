using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace UnitConverterApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConverterForm());
        }
    }

    internal class ConverterForm : Form
    {
        // Ordered unit -> factor-to-base-unit tables (meters for Length, grams for Weight).
        private static readonly Dictionary<string, List<KeyValuePair<string, double>>> UnitTables =
            new Dictionary<string, List<KeyValuePair<string, double>>>
            {
                {
                    "Length", new List<KeyValuePair<string, double>>
                    {
                        new KeyValuePair<string, double>("Millimeters (mm)", 0.001),
                        new KeyValuePair<string, double>("Centimeters (cm)", 0.01),
                        new KeyValuePair<string, double>("Meters (m)", 1),
                        new KeyValuePair<string, double>("Kilometers (km)", 1000),
                        new KeyValuePair<string, double>("Inches (in)", 0.0254),
                        new KeyValuePair<string, double>("Feet (ft)", 0.3048),
                        new KeyValuePair<string, double>("Yards (yd)", 0.9144),
                        new KeyValuePair<string, double>("Miles (mi)", 1609.344),
                    }
                },
                {
                    "Weight", new List<KeyValuePair<string, double>>
                    {
                        new KeyValuePair<string, double>("Milligrams (mg)", 0.001),
                        new KeyValuePair<string, double>("Grams (g)", 1),
                        new KeyValuePair<string, double>("Kilograms (kg)", 1000),
                        new KeyValuePair<string, double>("Ounces (oz)", 28.349523125),
                        new KeyValuePair<string, double>("Pounds (lb)", 453.59237),
                    }
                },
                {
                    "Temperature", new List<KeyValuePair<string, double>>
                    {
                        new KeyValuePair<string, double>("Celsius (°C)", double.NaN),
                        new KeyValuePair<string, double>("Fahrenheit (°F)", double.NaN),
                        new KeyValuePair<string, double>("Kelvin (K)", double.NaN),
                    }
                },
            };

        private static readonly Dictionary<string, string[]> DefaultFromTo = new Dictionary<string, string[]>
        {
            { "Length", new[] { "Millimeters (mm)", "Inches (in)" } },
            { "Weight", new[] { "Grams (g)", "Ounces (oz)" } },
            { "Temperature", new[] { "Celsius (°C)", "Fahrenheit (°F)" } },
        };

        private readonly ComboBox cboCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox txtInput = new TextBox();
        private readonly ComboBox cboFrom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button btnSwap = new Button();
        private readonly TextBox txtOutput = new TextBox { ReadOnly = true, TabStop = false };
        private readonly ComboBox cboTo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label lblError = new Label { ForeColor = Color.Firebrick, AutoSize = true };

        private bool updating;

        public ConverterForm()
        {
            Text = "Unit Converter";
            ClientSize = new Size(380, 240);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, 20), AutoSize = true };
            cboCategory.Location = new Point(120, 17);
            cboCategory.Width = 240;
            foreach (var cat in UnitTables.Keys) cboCategory.Items.Add(cat);

            var lblFrom = new Label { Text = "From:", Location = new Point(20, 65), AutoSize = true };
            txtInput.Location = new Point(120, 62);
            txtInput.Width = 100;
            cboFrom.Location = new Point(230, 62);
            cboFrom.Width = 130;

            btnSwap.Text = "⇅";
            btnSwap.Location = new Point(170, 105);
            btnSwap.Width = 50;
            btnSwap.Font = new Font("Segoe UI", 12);

            var lblTo = new Label { Text = "To:", Location = new Point(20, 150), AutoSize = true };
            txtOutput.Location = new Point(120, 147);
            txtOutput.Width = 100;
            txtOutput.BackColor = Color.White;
            cboTo.Location = new Point(230, 147);
            cboTo.Width = 130;

            lblError.Location = new Point(20, 195);

            Controls.AddRange(new Control[]
            {
                lblCategory, cboCategory, lblFrom, txtInput, cboFrom, btnSwap, lblTo, txtOutput, cboTo, lblError
            });

            cboCategory.SelectedIndexChanged += (s, e) => { PopulateUnitCombos(); UpdateResult(); };
            cboFrom.SelectedIndexChanged += (s, e) => UpdateResult();
            cboTo.SelectedIndexChanged += (s, e) => UpdateResult();
            txtInput.TextChanged += (s, e) => UpdateResult();
            btnSwap.Click += (s, e) =>
            {
                var f = cboFrom.SelectedItem;
                var t = cboTo.SelectedItem;
                updating = true;
                cboFrom.SelectedItem = t;
                cboTo.SelectedItem = f;
                updating = false;
                UpdateResult();
            };
            Shown += (s, e) => { txtInput.Focus(); txtInput.SelectAll(); };

            txtInput.Text = "1";
            cboCategory.SelectedItem = "Length";
        }

        private void PopulateUnitCombos()
        {
            var category = (string)cboCategory.SelectedItem;
            updating = true;
            cboFrom.Items.Clear();
            cboTo.Items.Clear();
            foreach (var kv in UnitTables[category])
            {
                cboFrom.Items.Add(kv.Key);
                cboTo.Items.Add(kv.Key);
            }
            var defaults = DefaultFromTo[category];
            cboFrom.SelectedItem = defaults[0];
            cboTo.SelectedItem = defaults[1];
            updating = false;
        }

        private void UpdateResult()
        {
            if (updating) return;
            if (cboFrom.SelectedItem == null || cboTo.SelectedItem == null) return;

            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                txtOutput.Text = "";
                return;
            }

            double value;
            if (!double.TryParse(txtInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                txtOutput.Text = "";
                lblError.Text = "Enter a number";
                return;
            }

            var category = (string)cboCategory.SelectedItem;
            var result = Convert(value, category, (string)cboFrom.SelectedItem, (string)cboTo.SelectedItem);
            txtOutput.Text = FormatResult(result);
        }

        private static double Convert(double value, string category, string fromUnit, string toUnit)
        {
            if (category == "Temperature")
            {
                double c = ToCelsius(value, fromUnit);
                return FromCelsius(c, toUnit);
            }

            var factors = UnitTables[category];
            double fromFactor = 0, toFactor = 0;
            foreach (var kv in factors)
            {
                if (kv.Key == fromUnit) fromFactor = kv.Value;
                if (kv.Key == toUnit) toFactor = kv.Value;
            }
            double baseValue = value * fromFactor;
            return baseValue / toFactor;
        }

        private static double ToCelsius(double value, string unit)
        {
            if (unit.StartsWith("Fahrenheit")) return (value - 32) * 5 / 9;
            if (unit.StartsWith("Kelvin")) return value - 273.15;
            return value;
        }

        private static double FromCelsius(double celsius, string unit)
        {
            if (unit.StartsWith("Fahrenheit")) return celsius * 9 / 5 + 32;
            if (unit.StartsWith("Kelvin")) return celsius + 273.15;
            return celsius;
        }

        private static string FormatResult(double value)
        {
            double rounded = Math.Round(value, 6);
            string text = rounded.ToString("0.######", CultureInfo.InvariantCulture);
            if (text == "" || text == "-0") text = "0";
            return text;
        }
    }
}
