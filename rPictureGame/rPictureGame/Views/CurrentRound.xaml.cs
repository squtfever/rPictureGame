﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Picturegame.Classes;
using Picturegame.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Picturegame.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentRound : ContentPage
    {
 
        private Round round;

        public CurrentRound()
        {
            InitializeComponent();
            LoadRound();
        }

        private async void LoadRound()
        {
            try
            {
                // Keep this method active whilest the page is loaded
                while (true)
                {
                    // Get round info from picturegame api
                    string pictureGameRequest = await new ApiRequests().MakeGetRequest<string>("https://api.picturegame.co/current");
                    round = JObject.Parse(pictureGameRequest)["round"].ToObject<Round>();
                    // Get post flair from reddit api
                    string redditGetRequest = await new ApiRequests().MakeGetRequest<string>("https://www.reddit.com/comments/" + round.Id + "/.json");
                    JArray jsonArray = JArray.Parse(redditGetRequest);
                    string flairtext = (string)jsonArray.SelectToken("$..data.children[0].data.link_flair_text");
                    // Check if title or post flair has changed to update layout
                    if (round.Title != CurrentTitle.Text || FlairLabel.Text != flairtext)
                    {
                        // Maybe change to binding in future
                        FlairLabel.Text = flairtext;
                        if (flairtext == "UNSOLVED") { FlairFrame.BackgroundColor = Color.LightBlue; }
                        else if (flairtext == "ROUND OVER") { FlairFrame.BackgroundColor = Color.Green; }
                        else { FlairFrame.BackgroundColor = Color.Red; }
                    }

                    UsernameLabel.Text = "u/" + round.HostName;
                    UsernameLabel.ClassId = round.HostName;
                    CurrentTitle.Text = round.Title;
                    CurrentImage.Source = round.ImageSource;
                    RedditBtn.IsEnabled = true;

                    // Check for updates every 15 seconds
                    await Task.Delay(15000);
                }
            
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                await DisplayAlert("Error", "The app has run into some issues. \n Make sure you have a valid connection or try again later.", "Ok");
            }
            
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            // Go to reddit post
            Device.OpenUri(new Uri("https://redd.it/" + round.Id));
        }

        private async void UsernameLabel_OnClicked(object sender, EventArgs e)
        {
            try
            {
                // Open modal with player info
                UserStats playermodal = new UserStats();
                playermodal.FillFromUsername(UsernameLabel.ClassId);
                await Navigation.PushModalAsync(playermodal);
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
        }
    }


   
}