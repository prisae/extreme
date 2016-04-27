using System;
using System.IO;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Porvem.Magnetotellurics;
using System.Text;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class PlainTextExporter
    {
        private readonly ResultsContainer _container;
        private readonly double _frequency;

        public bool ExportNames { get; set; }
        public bool PhaseInDegree { get; set; }
        public bool ReversePhaseSign { get; set; }

        public PlainTextExporter(ResultsContainer container, double frequency)
        {
            _container = container;
            _frequency = frequency;
        }

        public static string FrequencyToString(double frequency)
            => $"{frequency:0000000000.0000000000}";

        public static string GetResponsesFileName(double frequency)
            => $"responses_freq{FrequencyToString(frequency)}";

        public static string GetFieldsFileName(double frequency)
            => $"fields_freq{FrequencyToString(frequency)}";

        private void IterateThroughAll(Action<AllFieldsAtSite> action)
        {
            var lat = _container.Lateral;
			var nx = _container.LocalNx;
			var nx_shift = _container.LocalNxStart;

            foreach (var level in _container.LevelFields)
            {
                for (int i = 0; i < nx; i++)
                    for (int j = 0; j < lat.Ny; j++)
                    {
						var value = level.GetSite(lat, i, j,nx_shift);
                        action(value);
                    }
            }
        }

        private void WriteFreqAndCoordHead(StreamWriter sw)
        {
            string format = @"{0} {1} {2} {3} ";

            if (ExportNames)
                sw.Write(@"%name".PadLeft(14, ' '));
            else
                sw.Write(@"%");

            sw.Write(format,
                "x".PadLeft(10, ' '),
                "y".PadLeft(10, ' '),
                "z".PadLeft(10, ' '),
                "freq".PadLeft(14, ' '));
        }

        public void ExportRhoPhaseAndTipper(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                WriteFreqAndCoordHead(sw);

                // Rho
                WriteWithPadding(sw, "Rho_XX");
                WriteWithPadding(sw, "Rho_XY");
                WriteWithPadding(sw, "Rho_YX");
                WriteWithPadding(sw, "Rho_YY");

                // Phase
                WriteWithPadding(sw, "Phs_XX");
                WriteWithPadding(sw, "Phs_XY");
                WriteWithPadding(sw, "Phs_YX");
                WriteWithPadding(sw, "Phs_YY");


                // Tipper
                WriteWithPadding(sw, "W_ZX_RE");
                WriteWithPadding(sw, "W_ZX_IM");
                WriteWithPadding(sw, "W_ZY_RE");
                WriteWithPadding(sw, "W_ZY_IM");

                sw.WriteLine();

                IterateThroughAll(value =>
                {
                    WriteFreqAndCoord(sw, _frequency, value.Site);
                    WriteRhoAndPhase(sw, _frequency, value);
                    WriteTipperReIm(sw, value);

                    sw.WriteLine();
                });
            }
        }

        private void WriteTipperMag(StreamWriter sw, AllFieldsAtSite value)
        {
            Action<Complex> write = (c)
                  => sw.Write("{0} ",
                      c.Magnitude.ToString("E").PadLeft(14, ' '));

            var tipper = ResponseFunctionsCalculator.CalculateTipper(value);

            write(tipper.Zx);
            write(tipper.Zy);
        }

        private void WriteTipperReIm(StreamWriter sw, AllFieldsAtSite value)
        {
            Action<double> write = (c)
                  => sw.Write("{0} ",
                      c.ToString("E").PadLeft(14, ' '));

            var tipper = ResponseFunctionsCalculator.CalculateTipper(value);

            write(tipper.Zx.Real);
            write(tipper.Zx.Imaginary);
            write(tipper.Zy.Real);
            write(tipper.Zy.Imaginary);
        }

        public void ExportRawFields(string path)
		{
			if (_container.LocalNx > 0) {
				using (var sw = new StreamWriter (path,true,Encoding.ASCII,0x10000)) {
					WriteFreqAndCoordHead (sw);

					// normal pol X

					WriteWithPadding (sw, "N_polX_Ex_re");
					WriteWithPadding (sw, "N_polX_Ex_im");

					WriteWithPadding (sw, "N_polX_Ey_re");
					WriteWithPadding (sw, "N_polX_Ey_im");

					WriteWithPadding (sw, "N_polX_Ez_re");
					WriteWithPadding (sw, "N_polX_Ez_im");

					WriteWithPadding (sw, "N_polX_Hx_re");
					WriteWithPadding (sw, "N_polX_Hx_im");

					WriteWithPadding (sw, "N_polX_Hy_re");
					WriteWithPadding (sw, "N_polX_Hy_im");

					WriteWithPadding (sw, "N_polX_Hz_re");
					WriteWithPadding (sw, "N_polX_Hz_im");

					// normal pol Y 

					WriteWithPadding (sw, "N_polY_Ex_re");
					WriteWithPadding (sw, "N_polY_Ex_im");

					WriteWithPadding (sw, "N_polY_Ey_re");
					WriteWithPadding (sw, "N_polY_Ey_im");

					WriteWithPadding (sw, "N_polY_Ez_re");
					WriteWithPadding (sw, "N_polY_Ez_im");

					WriteWithPadding (sw, "N_polY_Hx_re");
					WriteWithPadding (sw, "N_polY_Hx_im");

					WriteWithPadding (sw, "N_polY_Hy_re");
					WriteWithPadding (sw, "N_polY_Hy_im");

					WriteWithPadding (sw, "N_polY_Hz_re");
					WriteWithPadding (sw, "N_polY_Hz_im");

					// anomaly pol X

					WriteWithPadding (sw, "A_polX_Ex_re");
					WriteWithPadding (sw, "A_polX_Ex_im");

					WriteWithPadding (sw, "A_polX_Ey_re");
					WriteWithPadding (sw, "A_polX_Ey_im");

					WriteWithPadding (sw, "A_polX_Ez_re");
					WriteWithPadding (sw, "A_polX_Ez_im");

					WriteWithPadding (sw, "A_polX_Hx_re");
					WriteWithPadding (sw, "A_polX_Hx_im");

					WriteWithPadding (sw, "A_polX_Hy_re");
					WriteWithPadding (sw, "A_polX_Hy_im");

					WriteWithPadding (sw, "A_polX_Hz_re");
					WriteWithPadding (sw, "A_polX_Hz_im");

					// anomaly pol Y      

					WriteWithPadding (sw, "A_polY_Ex_re");
					WriteWithPadding (sw, "A_polY_Ex_im");

					WriteWithPadding (sw, "A_polY_Ey_re");
					WriteWithPadding (sw, "A_polY_Ey_im");

					WriteWithPadding (sw, "A_polY_Ez_re");
					WriteWithPadding (sw, "A_polY_Ez_im");

					WriteWithPadding (sw, "A_polY_Hx_re");
					WriteWithPadding (sw, "A_polY_Hx_im");

					WriteWithPadding (sw, "A_polY_Hy_re");
					WriteWithPadding (sw, "A_polY_Hy_im");

					WriteWithPadding (sw, "A_polY_Hz_re");
					WriteWithPadding (sw, "A_polY_Hz_im");


					sw.WriteLine ();

					IterateThroughAll (value => {
						WriteFreqAndCoord (sw, _frequency, value.Site);
						WriteFullField (sw, value);

						sw.WriteLine ();
					});
				}
			}
		}


        public void ExportMtResponses(string path)
        {
            using (var sw = new StreamWriter(path))
            {
				
                WriteFreqAndCoordHead(sw);

                // Impedance

                WriteWithPadding(sw, "Z_XX_re");
                WriteWithPadding(sw, "Z_XX_im");

                WriteWithPadding(sw, "Z_XY_re");
                WriteWithPadding(sw, "Z_XY_im");

                WriteWithPadding(sw, "Z_YX_re");
                WriteWithPadding(sw, "Z_YX_im");

                WriteWithPadding(sw, "Z_YY_re");
                WriteWithPadding(sw, "Z_YY_im");

                // Rho

                WriteWithPadding(sw, "Rho_XX");
                WriteWithPadding(sw, "Rho_XY");
                WriteWithPadding(sw, "Rho_YX");
                WriteWithPadding(sw, "Rho_YY");

                // Phase

                WriteWithPadding(sw, "Phs_XX");
                WriteWithPadding(sw, "Phs_XY");
                WriteWithPadding(sw, "Phs_YX");
                WriteWithPadding(sw, "Phs_YY");

                // Tipper

                WriteWithPadding(sw, "W_ZX_re");
                WriteWithPadding(sw, "W_ZX_im");
                WriteWithPadding(sw, "W_ZY_re");
                WriteWithPadding(sw, "W_ZY_im");


                // Electric Tipper

                WriteWithPadding(sw, "EW_ZX_re");
                WriteWithPadding(sw, "EW_ZX_im");
                WriteWithPadding(sw, "EW_ZY_re");
                WriteWithPadding(sw, "EW_ZY_im");

                sw.WriteLine();

                IterateThroughAll(value =>
                {
                    WriteFreqAndCoord(sw, _frequency, value.Site);

                    var impedance = ResponseFunctionsCalculator.CalculateImpedanceTensor(value);
                    var tipper = ResponseFunctionsCalculator.CalculateTipper(value);
                    var eTipper = ResponseFunctionsCalculator.CalculateElectricTipper(value);

                    WriteImpedance(sw, impedance);
                    WriteRhoAndPhase(sw, _frequency, value);
                    WriteTipper(sw, tipper);
                    WriteTipper(sw, eTipper);

                    sw.WriteLine();
                });
            }
        }


        private void WriteFreqAndCoord(StreamWriter sw, double frequency, ObservationSite site)
        {
            var name = site.Name.PadLeft(15, ' ');

            var xStr = site.X.ToString("F2").PadLeft(10, ' ');
            var yStr = site.Y.ToString("F2").PadLeft(10, ' ');
            var zStr = site.Z.ToString("F2").PadLeft(10, ' ');

            if (ExportNames)
                sw.Write("{0} ", name);

            sw.Write(" {0} {1} {2} ", xStr, yStr, zStr);
            sw.Write("{0} ", frequency.ToString("E").PadLeft(14, ' '));
        }

        private void WriteRhoAndPhase(StreamWriter sw, double freq, AllFieldsAtSite value)
        {
            var omega = OmegaModelUtils.FrequencyToOmega(freq);
            var impedance = ResponseFunctionsCalculator.CalculateImpedanceTensor(value);

            WriteApparentResistivity(sw, impedance, omega);
            WritePhase(sw, impedance);
        }

        private static void WriteWithPadding(StreamWriter sw, string text)
        {
            sw.Write("{0} ", text.PadLeft(14, ' '));
        }

        private static void WriteTipper(StreamWriter sw, Tipper tipper)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} {1} ",
                       c.Real.ToString("E").PadLeft(14, ' '),
                       c.Imaginary.ToString("E").PadLeft(14, ' '));

            write(tipper.Zx);
            write(tipper.Zy);
        }

        private static void WriteImpedance(StreamWriter sw, ImpedanceTensor impedance)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} {1} ",
                       c.Real.ToString("E").PadLeft(14, ' '),
                       c.Imaginary.ToString("E").PadLeft(14, ' '));

            write(impedance.Xx);
            write(impedance.Xy);
            write(impedance.Yx);
            write(impedance.Yy);
        }

        private double ProcessPhase(Complex value)
        {
            var result = value.Phase;

            if (PhaseInDegree)
                result *= (180 / Math.PI);

            if (ReversePhaseSign)
                result = -result;

            return result;
        }

        private void WritePhase(StreamWriter sw, ImpedanceTensor impedance)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} ", ProcessPhase(c).ToString("E").PadLeft(14, ' '));

            write(impedance.Xx);
            write(impedance.Xy);
            write(impedance.Yx);
            write(impedance.Yy);
        }

        private void WritePhaseXyYx(StreamWriter sw, ImpedanceTensor impedance)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} ", ProcessPhase(c).ToString("E").PadLeft(14, ' '));

            write(impedance.Xy);
            write(impedance.Yx);
        }

        private static void WriteApparentResistivity(StreamWriter sw, ImpedanceTensor impedance, double omega)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} ", CalculateApparentResistivity(c, omega).ToString("E").PadLeft(14, ' '));

            write(impedance.Xx);
            write(impedance.Xy);
            write(impedance.Yx);
            write(impedance.Yy);
        }


        private static void WriteApparentResistivityXyYx(StreamWriter sw, ImpedanceTensor impedance, double omega)
        {
            Action<Complex> write = (c)
                   => sw.Write("{0} ", CalculateApparentResistivity(c, omega).ToString("E").PadLeft(14, ' '));

            write(impedance.Xy);
            write(impedance.Yx);
        }


        private const double Mu0 = (4.0 * Math.PI * 1.0E-07);

        private static double CalculateApparentResistivity(Complex z, double omega)
        {
            return (z.Magnitude * z.Magnitude) / (omega * Mu0);
        }

        private static void WriteFullField(StreamWriter sw, AllFieldsAtSite fv)
        {
            Action<Complex> write = (c)
                => sw.Write("{0} {1} ",
                    c.Real.ToString("E").PadLeft(14, ' '),
                    c.Imaginary.ToString("E").PadLeft(14, ' '));

            Action<ComplexVector> writeVector = (cv)
                =>
            {
                write(cv.X);
                write(cv.Y);
                write(cv.Z);
            };

            writeVector(fv.NormalE1);
            writeVector(fv.NormalH1);
            writeVector(fv.NormalE2);
            writeVector(fv.NormalH2);

            writeVector(fv.AnomalyE1);
            writeVector(fv.AnomalyH1);
            writeVector(fv.AnomalyE2);
            writeVector(fv.AnomalyH2);

        }
    }
}
