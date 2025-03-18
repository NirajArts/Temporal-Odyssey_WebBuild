//SPDX-License-Identifier: MIT
pragma solidity 0.8.28;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract Game is ERC721, ERC721URIStorage, Ownable {
    uint256 private _tokenIds;

    mapping(address => mapping(uint256 => bool)) public completedLevels;

    mapping(address => uint256) public playerScores;

    mapping(address => bool) public gameAdmins;

    event LevelCompleted(address player, uint256 levelId, uint256 tokenId);
    event ScoreUpdated(address player, uint256 increment, uint256 newScore);

    modifier onlyGameAdmin() {
        require(
            gameAdmins[msg.sender] || owner() == msg.sender,
            "Not authorized"
        );
        _;
    }

    constructor() ERC721("LevelCompletionNFT", "LCNFT") Ownable(msg.sender) {}

    function setGameAdmin(address admin, bool status) public onlyOwner {
        gameAdmins[admin] = status;
    }

    function updateScore(
        address player,
        uint256 incrementAmount
    ) public onlyGameAdmin returns (uint256) {
        playerScores[player] += incrementAmount;

        emit ScoreUpdated(player, incrementAmount, playerScores[player]);

        return playerScores[player];
    }

    function mintNFT(
        address player,
        uint256 levelId,
        string memory metadataURI
    ) public onlyGameAdmin returns (uint256) {
        require(bytes(metadataURI).length > 0, "Metadata URI is required");
        require(!completedLevels[player][levelId], "Level already completed");

        _tokenIds++;
        uint256 newTokenId = _tokenIds;

        _mint(player, newTokenId);
        _setTokenURI(newTokenId, metadataURI);

        completedLevels[player][levelId] = true;
        emit LevelCompleted(player, levelId, newTokenId);
        return newTokenId;
    }

    function hasCompletedLevel(
        address player,
        uint256 levelId
    ) public view returns (bool) {
        return completedLevels[player][levelId];
    }

    function tokenURI(
        uint256 tokenId
    ) public view override(ERC721, ERC721URIStorage) returns (string memory) {
        return super.tokenURI(tokenId);
    }

    function getPlayerScore(address player) public view returns (uint256) {
        return playerScores[player];
    }

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(ERC721, ERC721URIStorage) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
