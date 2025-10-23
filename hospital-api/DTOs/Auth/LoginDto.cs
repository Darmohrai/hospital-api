﻿using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Auth;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}