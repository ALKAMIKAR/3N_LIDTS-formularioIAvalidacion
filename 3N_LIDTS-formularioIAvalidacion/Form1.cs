using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3N_LIDTS_formularioIAvalidacion
{
    public partial class Form1 : Form
    {
        // Archivo donde se guardarán los registros.
        private readonly string rutaArchivo;

        // Acepta letras, espacios, guiones y apóstrofes.
        // También acepta letras con acentos y la ñ.
        private static readonly Regex PatronTexto =
            new Regex(@"^[\p{L} '-]+$", RegexOptions.Compiled);
        public Form1()
        {
            InitializeComponent();

            // Guarda el TXT en la carpeta donde se ejecuta el programa.
            rutaArchivo = Path.Combine(
                Application.StartupPath,
                "registros.txt"
            );

            // Validar cuando el usuario sale de cada TextBox.
            tbxName.Leave += ValidarNombre;
            tbxApellidos.Leave += ValidarApellido;
            tbxEdad.Leave += ValidarEdad;
            tbxEstatura.Leave += ValidarEstatura;
            tbxTelefono.Leave += ValidarTelefono;

        }
        // ---------------------------------------------------------
        // VALIDACIONES VISUALES
        // ---------------------------------------------------------

        private void ValidarNombre(object sender, EventArgs e)
        {
            TextBox campo = (TextBox)sender;
            PintarCampo(campo, EsTextoValido(campo.Text), false);
        }

        private void ValidarApellido(object sender, EventArgs e)
        {
            TextBox campo = (TextBox)sender;
            PintarCampo(campo, EsTextoValido(campo.Text), false);
        }

        private void ValidarEdad(object sender, EventArgs e)
        {
            TextBox campo = (TextBox)sender;
            PintarCampo(campo, EsEnteroValido(campo.Text), false);
        }

        private void ValidarEstatura(object sender, EventArgs e)
        {
            TextBox campo = (TextBox)sender;
            PintarCampo(campo, EsDecimalValido(campo.Text), false);
        }

        private void ValidarTelefono(object sender, EventArgs e)
        {
            TextBox campo = (TextBox)sender;
            PintarCampo(campo, TelefonoValido(campo.Text), false);
        }

        // Cambia el color de un campo dependiendo de su validez.
        private void PintarCampo(
            TextBox campo,
            bool valido,
            bool validarVacio)
        {
            if (!validarVacio &&
                string.IsNullOrWhiteSpace(campo.Text))
            {
                campo.BackColor = SystemColors.Window;
                return;
            }

            campo.BackColor = valido
                ? Color.LightGreen
                : Color.MistyRose;
        }

        // ---------------------------------------------------------
        // MÉTODOS QUE COMPRUEBAN LOS DATOS
        // ---------------------------------------------------------

        private bool EsTextoValido(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            string texto = valor.Trim();

            return PatronTexto.IsMatch(texto)
                && texto.Any(char.IsLetter);
        }

        private bool EsEnteroValido(string valor)
        {
            int resultado;

            return int.TryParse(
                valor.Trim(),
                out resultado
            ) && resultado >= 1 && resultado <= 120;
        }

        private bool EsDecimalValido(string valor)
        {
            decimal resultado;

            return decimal.TryParse(
                valor.Trim(),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.CurrentCulture,
                out resultado
            ) && resultado > 0;
        }

        private bool TelefonoValido(string valor)
        {
            string telefono = valor.Trim();

            return telefono.Length == 10
                && telefono.All(
                    caracter => caracter >= '0' && caracter <= '9'
                );
        }

        private string ObtenerGenero()
        {
            if (rbMujer.Checked)
                return "Femenino";

            if (rbHombre.Checked)
                return "Masculino";

            if (rbOtros.Checked)
                return "Otro";

            return "";
        }

        // ---------------------------------------------------------
        // VALIDACIÓN COMPLETA ANTES DE GUARDAR
        // ---------------------------------------------------------

        private bool ValidarFormulario()
        {
            bool nombreValido = EsTextoValido(tbxName.Text);
            bool apellidosValidos = EsTextoValido(tbxApellidos.Text);
            bool telefonoValido = TelefonoValido(tbxTelefono.Text);
            bool edadValida = EsEnteroValido(tbxEdad.Text);
            bool estaturaValida = EsDecimalValido(tbxEstatura.Text);
            bool generoValido = !string.IsNullOrEmpty(ObtenerGenero());

            // Mostrar en color los campos que necesitan corregirse.
            PintarCampo(tbxName, nombreValido, true);
            PintarCampo(tbxApellidos, apellidosValidos, true);
            PintarCampo(tbxTelefono, telefonoValido, true);
            PintarCampo(tbxEdad, edadValida, true);
            PintarCampo(tbxEstatura, estaturaValida, true);

            List<string> errores = new List<string>();

            if (!nombreValido)
                errores.Add("Nombre");

            if (!apellidosValidos)
                errores.Add("Apellidos");

            if (!telefonoValido)
                errores.Add("Teléfono: debe contener 10 dígitos.");

            if (!edadValida)
                errores.Add("Edad: introduce un entero entre 1 y 120.");

            if (!estaturaValida)
                errores.Add("Estatura: introduce un decimal positivo.");

            if (!generoValido)
                errores.Add("Género: selecciona una opción.");

            if (errores.Count > 0)
            {
                MessageBox.Show(
                    "Corrige los siguientes campos antes de guardar:\n\n- "
                    + string.Join("\n- ", errores),
                    "Datos incorrectos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Colocar el cursor en el primer campo incorrecto.
                if (!nombreValido)
                    tbxName.Focus();
                else if (!apellidosValidos)
                    tbxApellidos.Focus();
                else if (!telefonoValido)
                    tbxTelefono.Focus();
                else if (!edadValida)
                    tbxEdad.Focus();
                else if (!estaturaValida)
                    tbxEstatura.Focus();
                else if (!generoValido)
                    rbMujer.Focus();

                return false;
            }

            return true;
        }

        // ---------------------------------------------------------
        // BOTÓN GUARDAR
        // Se conserva el nombre original del evento.
        // ---------------------------------------------------------

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            string nombre = tbxName.Text.Trim();
            string apellidos = tbxApellidos.Text.Trim();
            string telefono = tbxTelefono.Text.Trim();
            string edad = tbxEdad.Text.Trim();
            string estatura = tbxEstatura.Text.Trim();
            string genero = ObtenerGenero();

            string salto = Environment.NewLine;

            string datos =
                $"Nombre: {nombre}{salto}" +
                $"Apellidos: {apellidos}{salto}" +
                $"Teléfono: {telefono}{salto}" +
                $"Edad: {edad}{salto}" +
                $"Estatura: {estatura}{salto}" +
                $"Género: {genero}";

            // Cada registro incluye fecha y hora.
            string registro =
                $"Fecha de registro: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"
                + salto
                + datos
                + salto
                + "----------------------------------------"
                + salto
                + salto;

            try
            {
                // Crea el archivo si no existe.
                // Si ya existe, agrega el nuevo registro al final.
                File.AppendAllText(
                    rutaArchivo,
                    registro,
                    new UTF8Encoding(false)
                );

                MessageBox.Show(
                    "Registro guardado correctamente.\n\n"
                    + datos
                    + "\n\nArchivo: "
                    + rutaArchivo,
                    "Confirmación de registro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible guardar el registro.\n\n"
                    + "Detalles: " + ex.Message,
                    "Error al guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            TextBox[] campos =
            {
                tbxName,
                tbxApellidos,
                tbxTelefono,
                tbxEdad,
                tbxEstatura
            };

            foreach (TextBox campo in campos)
            {
                campo.Clear();
                campo.BackColor = SystemColors.Window;
            }

            rbMujer.Checked = false;
            rbHombre.Checked = false;
            rbOtros.Checked = false;

            tbxName.Focus();
        }
    
}
    }

