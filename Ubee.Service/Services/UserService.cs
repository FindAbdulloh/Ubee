﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ubee.Data.IRepositories;
using Ubee.Data.Repositories;
using Ubee.Domain.Configurations;
using Ubee.Domain.Entities;
using Ubee.Service.DTOs;
using Ubee.Service.Extensions;
using Ubee.Service.Helpers;
using Ubee.Service.Interfaces;

namespace Ubee.Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository = new UserRepository();
    private readonly IMapper mapper;
    public UserService(IMapper mapper)
    {
        this.mapper = mapper;
    }

    public async ValueTask<Response<UserDto>> AddUserAsync(UserForCreationDto userForCreationDto)
    {
        var user = await this.userRepository.SelectUserAsync(user =>
            user.Username.Equals(userForCreationDto.Username) ||
            user.Phone.Equals(userForCreationDto.Phone));

        if (user is not null)
            return new Response<UserDto>
            {
                Code = 404,
                Message = "User is already existed",
                Value = (UserDto)user
            };


        var mappedUser = this.mapper.Map<User>(userForCreationDto);
        mappedUser.Password = userForCreationDto.Password.Encrypt();
        var addedUser = await this.userRepository.InsertUserAsync(mappedUser);
        var resultDto = this.mapper.Map<UserDto>(addedUser);
        return new Response<UserDto>
        {
            Code = 200,
            Message = "Success",
            Value = resultDto
        };
    }

    public async ValueTask<Response<bool>> DeleteUserAsync(long id)
    {
        User user = await this.userRepository.SelectUserAsync(user => user.Id.Equals(id));
        if (user is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Couldn't find for given ID",
                Value = false
            };

        await this.userRepository.DeleteUserAysnyc(id);
        return new Response<bool>
        {
            Code = 200,
            Message = "Success",
            Value = true
        };
    }

    public async ValueTask<Response<List<UserDto>>> GetAllUserAsync(PaginationParams @params, string search = null)
    {
        var users = await this.userRepository.SelectAllUsers().ToPagedList(@params).ToListAsync();
        if (users.Any())
            return new Response<List<UserDto>>
            {
                Code = 404,
                Message = "Success",
                Value = null
            };

        var result = users.Where(user => user.Firstname.Contains(search, StringComparison.OrdinalIgnoreCase));
        var mappedUsers = this.mapper.Map<List<UserDto>>(result);
        return new Response<List<UserDto>>
        {
            Code = 200,
            Message = "Success",
            Value = mappedUsers
        };
    }

    public async ValueTask<Response<UserDto>> GetUserByIdAsync(long id)
    {
        User user = await this.userRepository.SelectUserAsync(user => user.Id.Equals(id));
        if (user is null)
            return new Response<UserDto>
            {
                Code = 404,
                Message = "Couldn't find for given ID",
                Value = null
            };

        var mappedUsers = this.mapper.Map<UserDto>(user);
        return new Response<UserDto>
        {
            Code = 200,
            Message = "Success",
            Value = mappedUsers
        };
    }

    public async ValueTask<Response<UserDto>> ModifyUserAsync(long id, UserForCreationDto userForCreationDto)
    {
        User user = await this.userRepository.SelectUserAsync(user => user.Id.Equals(id));
        if (user is null)
            return new Response<UserDto>
            {
                Code = 404,
                Message = "Couldn't find for given ID",
                Value = null
            };

        var updatedUser = await this.userRepository.UpdateUserAsync(user);
        var mappedUsers = this.mapper.Map<UserDto>(updatedUser);
        return new Response<UserDto>
        {
            Code = 200,
            Message = "Success",
            Value = mappedUsers
        };
    }
}
