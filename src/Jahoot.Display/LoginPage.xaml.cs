using Jahoot.Core.Models;
﻿using Jahoot.Display.Services;
﻿using System.Windows;
﻿
﻿namespace Jahoot.Display
﻿{
﻿    /// <summary>
﻿    /// This is the main window of our app. It's where users can log in.
﻿    /// </summary>
﻿    public partial class MainWindow : Window
﻿    {
﻿        // This is our special service that handles all the login and logout stuff.
﻿        private readonly IAuthService _authService;
﻿
﻿        /// <summary>
﻿        /// Sets up the main window. It gets the login service ready.
﻿        /// </summary>
﻿        /// <param name="authService">The login service we need.</param>
﻿        public MainWindow(IAuthService authService)
﻿        {
﻿            InitializeComponent(); // Get all the buttons and text boxes ready.
﻿            _authService = authService; // Keep hold of the login service.
﻿        }
﻿
﻿        /// <summary>
﻿        /// This happens when the "Login" button is clicked.
﻿        /// It tries to log the user in.
﻿        /// </summary>
﻿        /// <param name="sender">The button that was clicked.</param>
﻿        /// <param name="e">Extra info about the click.</param>
﻿        private async void LoginButton_Click(object sender, RoutedEventArgs e)
﻿        {
﻿            // Grab the email and password the user typed in.
﻿            var loginRequest = new LoginRequest
﻿            {
﻿                Email = UsernameTextBox.Text, // Get the email from the text box.
﻿                Password = PasswordBox.Password // Get the password from the password box.
﻿            };
﻿
﻿            // Ask the login service to try and log us in.
﻿            var (success, message) = await _authService.Login(loginRequest);
﻿
﻿            // Tell the user if it worked or not.
﻿            if (success)
﻿            {
﻿                MessageBox.Show("Login successful!"); // Yay, we're in!
﻿            }
﻿            else
﻿            {
﻿                MessageBox.Show($"Login failed: {message}"); // Boo, something went wrong. Show the error message.
﻿            }
﻿        }
﻿    }
﻿}
﻿