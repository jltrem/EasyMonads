﻿using System;
using System.Threading.Tasks;

namespace EasyMonads
{
   public readonly struct Maybe<TValue>
   {
      private readonly MaybeState _state;
      private readonly TValue _value;

      private Maybe(TValue value)
      {
         _value = value;
         _state = value is null
            ? MaybeState.None
            : MaybeState.Some;
      }

      public static Maybe<TValue> From(TValue value)
      {
         return new Maybe<TValue>(value);
      }

      public static Task<Maybe<TValue>> FromAsync(Task<TValue> value)
      {
         async Task<Maybe<TValue>> unpack()
         {
            var result = await value;
            return From(result);
         }

         return unpack();
      }

      public bool IsSome
      { get { return _state == MaybeState.Some; } }

      public bool IsNone
      { get { return _state == MaybeState.None; } }

      public Unit IfSome(Action<TValue> some)
      {
         if (some is null)
         {
            throw new ArgumentNullException(nameof(some));
         }

         if (IsSome)
         {
            some(_value);
         }

         return default;
      }

      public async Task<Unit> IfSomeAsync(Func<TValue, Task> someAsync)
      {
         if (someAsync is null)
         {
            throw new ArgumentNullException(nameof(someAsync));
         }

         if (IsSome)
         {
            await someAsync(_value);
         }

         return default;
      }

      public Unit IfNone(Action none)
      {
         if (none is null)
         {
            throw new ArgumentNullException(nameof(none));
         }

         if (IsNone)
         {
            none();
         }

         return default;
      }

      public TValue SomeOrDefault(TValue defaultValue)
      {
         return IsNone
            ? defaultValue
            : _value;
      }

      public TResult Match<TResult>(Func<TResult> none, Func<TValue, TResult> some)
      {
         if (none is null)
         {
            throw new ArgumentNullException(nameof(none));
         }

         if (some is null)
         {
            throw new ArgumentNullException(nameof(some));
         }

         return IsSome
            ? some(_value)
            : none();
      }

      public async Task<TResult> MatchAsync<TResult>(Func<TResult> none, Func<TValue, Task<TResult>> someAsync)
      {
         if (none is null)
         {
            throw new ArgumentNullException(nameof(none));
         }

         if (someAsync is null)
         {
            throw new ArgumentNullException(nameof(someAsync));
         }

         return IsSome
            ? await someAsync(_value)
            : none();
      }

      public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> noneAsync, Func<TValue, TResult> some)
      {
         if (noneAsync is null)
         {
            throw new ArgumentNullException(nameof(noneAsync));
         }

         if (some is null)
         {
            throw new ArgumentNullException(nameof(some));
         }

         return IsSome
            ? some(_value)
            : await noneAsync();
      }

      public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> noneAsync, Func<TValue, Task<TResult>> someAsync)
      {
         if (noneAsync is null)
         {
            throw new ArgumentNullException(nameof(noneAsync));
         }

         if (someAsync is null)
         {
            throw new ArgumentNullException(nameof(someAsync));
         }

         return IsSome
            ? await someAsync(_value)
            : await noneAsync();
      }

      public Maybe<TResult> Map<TResult>(Func<TValue, TResult> map)
      {
         if (map is null)
         {
            throw new ArgumentNullException(nameof(map));
         }

         return IsSome
            ? map(_value)
            : Maybe<TResult>.None;
      }

      public async Task<Maybe<TResult>> MapAsync<TResult>(Func<TValue, Task<TResult>> mapAsync)
      {
         if (mapAsync is null)
         {
            throw new ArgumentNullException(nameof(mapAsync));
         }

         return IsSome
            ? await mapAsync(_value)
            : Maybe<TResult>.None;
      }

      public Maybe<TResult> Bind<TResult>(Func<TValue, Maybe<TResult>> bind)
      {
         if (bind is null)
         {
            throw new ArgumentNullException(nameof(bind));
         }

         return IsSome
            ? bind(_value)
            : Maybe<TResult>.None;
      }

      public async Task<Maybe<TResult>> BindAsync<TResult>(Func<TValue, Task<Maybe<TResult>>> bindAsync)
      {
         if (bindAsync is null)
         {
            throw new ArgumentNullException(nameof(bindAsync));
         }

         return IsSome
            ? await bindAsync(_value)
            : Maybe<TResult>.None;
      }

      public Either<TLeft, TValue> ToEither<TLeft>(TLeft left)
      {
         return IsSome
            ? Either<TLeft, TValue>.FromRight(_value)
            : Either<TLeft, TValue>.FromLeft(left);
      }

      public Either<TValue, Unit> ToLeftEither()
      {
         return IsSome
            ? Either<TValue, Unit>.FromLeft(_value)
            : Either<TValue, Unit>.FromRight(Unit.Default);
      }

      public Either<TValue, TRight> ToLeftEither<TRight>(TRight right)
      {
         return IsSome
            ? Either<TValue, TRight>.FromLeft(_value)
            : Either<TValue, TRight>.FromRight(right);
      }

      public Maybe<TResult> Select<TResult>(Func<TValue, TResult> map)
      {
         return IsSome
            ? Maybe<TResult>.From(map(_value))
            : default;
      }

      public Maybe<TResult> SelectMany<TIntermediate, TResult>(Func<TValue, Maybe<TIntermediate>> bind, Func<TValue, TIntermediate, TResult> project)
      {
         if (IsNone)
         {
            return default;
         }

         var bound = bind(_value);

         if (bound.IsNone)
         {
            return default;
         }

         var result = project(_value, bound._value);

         if (result is null)
         {
            throw new InvalidOperationException();
         }

         return result;
      }

      public Maybe<TValue> Where(Func<TValue, bool> predicate)
      {
         return IsSome && predicate(_value)
            ? this
            : default;
      }

      public static readonly Maybe<TValue> None = default;
      public static implicit operator Maybe<TValue>(TValue value) => Maybe<TValue>.From(value);
   }
}
