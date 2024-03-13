using System.Threading.Tasks;
using UniDex.Pokemons.API;
using UniDex.Pokemons.API.Data;
using UnityEngine;

namespace UniDex.Pokemons
{
    public static class PokemonFactory
    {
        public static async Task<PokemonObject> CreatePokemonFromAPI(string pokemonNameOrID)
        {
            var pokemonResult = await PokemonAPI.GetPokemon(pokemonNameOrID);
            if (pokemonResult.IsError)
            {
                throw new System.Exception($"Couldn't fetch Pokemon - {pokemonNameOrID}. Error: {pokemonResult.Error}");
            }

            Pokemon pokemon = pokemonResult.Data;
            var pokemonSpeciesTask = PokemonAPI.GetPokemonSpecies(pokemon);
            string spriteURL = pokemon.sprites.front_default;
            Task<Texture> textureTask = null;
            if (!string.IsNullOrEmpty(spriteURL))
            {
                textureTask = TextureDownloader.DownloadFromURL(spriteURL);
            }

            if (textureTask != null)
            {
                await Task.WhenAll(pokemonSpeciesTask, textureTask);
            }
            else
            {
                await pokemonSpeciesTask;
            }

            var pokemonSpeciesResult = pokemonSpeciesTask.Result;
            if (pokemonSpeciesResult.IsError)
            {
                throw new System.Exception($"Couldn't fetch Pokemon Species - {pokemonNameOrID}. Error: {pokemonSpeciesResult.Error}");
            }

            PokemonSpecies pokemonSpecies = pokemonSpeciesResult.Data;
            Texture pokemonTexture = textureTask?.Result;
            if (pokemonTexture)
            {
                pokemonTexture.filterMode = FilterMode.Point;
            }

            return new PokemonObject(pokemon, pokemonSpecies, pokemonTexture);
        }

        public static async Task<PokemonObject> CreatePokemonFromAPI(uint pokemonID)
        {
            return await CreatePokemonFromAPI(pokemonID.ToString());
        } 
    }
}
