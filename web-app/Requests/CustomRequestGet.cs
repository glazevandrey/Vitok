﻿namespace web_app.Requests
{
    public class CustomRequestGet
    {
        public string Address { get; set; }
        public object Args { get; set; }
        public CustomRequestGet(string address, object args)
        {
            Address = address;
            Args = args;
        }
    }
}