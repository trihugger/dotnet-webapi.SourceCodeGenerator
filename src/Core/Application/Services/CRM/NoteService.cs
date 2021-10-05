// Autogenerated by SourceCodeGenerator

using DN.WebApi.Application.Abstractions.Repositories;
using DN.WebApi.Application.Abstractions.Services.CRM;
using DN.WebApi.Application.Abstractions.Services.General;
using DN.WebApi.Application.Exceptions;
using DN.WebApi.Application.Specifications;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Domain.Entities.CRM;
using DN.WebApi.Domain.Enums;
using DN.WebApi.Shared.DTOs.CRM;
using Mapster;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DN.WebApi.Application.Services.CRM
{
    public class NoteService : INoteService
    {
        private readonly IStringLocalizer<NoteService> _localizer;
        private readonly IFileStorageService _file;
        private readonly IRepositoryAsync _repository;

        public NoteService(IRepositoryAsync repository, IStringLocalizer<NoteService> localizer, IFileStorageService file)
        {
            _repository = repository;
            _localizer = localizer;
            _file = file;
        }

        public async Task<Result<Guid>> CreateNoteAsync(CreateNoteRequest request)
        {
            var noteExists = await _repository.ExistsAsync<Note>(a => a.Subject == request.Subject);
            if (noteExists) throw new EntityAlreadyExistsException(string.Format(_localizer["note.alreadyexists"], request.Subject));
            var personExists = await _repository.ExistsAsync<Person>(a => a.Id == request.PersonId);
            if (!personExists) throw new EntityNotFoundException(string.Format(_localizer["person.notfound"], request.PersonId));
            var note = new Note(request.Subject, request.Message, request.PersonId);
            var noteId = await _repository.CreateAsync<Note>(note);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(noteId);
        }

        public async Task<Result<Guid>> UpdateNoteAsync(UpdateNoteRequest request, Guid id)
        {
            var note = await _repository.GetByIdAsync<Note>(id, null);
            if (note == null) throw new EntityNotFoundException(string.Format(_localizer["note.notfound"], id));
            var personExists = await _repository.ExistsAsync<Person>(a => a.Id == request.PersonId);
            if (!personExists) throw new EntityNotFoundException(string.Format(_localizer["person.notfound"], request.PersonId));
            var updatedNote = note.Update(request.Subject, request.Message, request.PersonId);
            await _repository.UpdateAsync<Note>(updatedNote);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(id);
        }

        public async Task<Result<Guid>> DeleteNoteAsync(Guid id)
        {
            await _repository.RemoveByIdAsync<Note>(id);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(id);
        }

        public async Task<Result<NoteDetailsDto>> GetNoteDetailsAsync(Guid id)
        {
            var spec = new BaseSpecification<Note>();
            spec.Includes.Add(a => a.Person);
            var note = await _repository.GetByIdAsync<Note, NoteDetailsDto>(id, spec);
            return await Result<NoteDetailsDto>.SuccessAsync(note);
        }

        public async Task<PaginatedResult<NoteDto>> GetNotesAsync(NoteListFilter filter)
        {
            var notes = await _repository.GetPaginatedListAsync<Note, NoteDto>(filter.PageNumber, filter.PageSize, filter.OrderBy, filter.Search);
            return notes;
        }

        public async Task<Result<NoteDto>> GetByIdUsingDapperAsync(Guid id)
        {
            var note = await _repository.QueryFirstOrDefaultAsync<Note>($"SELECT * FROM public.\"Notes\" WHERE \"Id\"  = '{id}' AND \"TenantKey\"='@tenantKey'");
            var mappedNote = note.Adapt<NoteDto>();
            return await Result<NoteDto>.SuccessAsync(mappedNote);
        }
    }
}