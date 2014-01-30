﻿namespace Solvberget.Core.DTOs
{
    public class RequestReplyDto
    {
        public RequestReplyDto()
        {
            Success = true;
        }

        public bool Success { get; set; }
        public string Reply { get; set; }
    }

	public static class Replies
	{
		public static string RequireLoginReply = "RequireLogin";
	}
}
