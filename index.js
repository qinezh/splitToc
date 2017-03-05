"use strict";

let fs = require("fs-extra");
let path = require("path");
let yaml = require("js-yaml");

main();

function main() {
    let config = {
        "yamlOption": {
            "lineWidth": -1
        },
        "splitFolderName": "_splitted"
    }

    let args = process.argv.slice(2);
    if (args.length !== 1) {
        throw new Error("Please input the original path of toc.yml which needed to be splitted.");
    }

    splitToc(args[0], config);
}

function splitToc(originalTocPath, config) {
    if (!path.isAbsolute(originalTocPath)) {
        originalTocPath = path.normalize(originalTocPath);
    }

    if (!fs.existsSync(originalTocPath)) {
        throw new Error(`the path of toc file: ${originalTocPath} can't be found.`);
    }

    let originalTocDir = path.dirname(originalTocPath);
    let content = fs.readFileSync(originalTocPath, "utf8");

    let tocModel;
    try {
        tocModel = yaml.safeLoad(content);
    } catch (err) {
        console.log(`Error occurs while parsing toc ${originalTocPath}, please check if the format is correct.`);
        throw err;
    }

    let mergedTocModel = [];
    tocModel.forEach(ns => {
        let splittedTocPath = path.join(originalTocDir, config.splitFolderName, ns.uid, "toc.yml");
        let splittedTocRelativePath = path.relative(originalTocDir, splittedTocPath);
        let splittedTocDir = path.dirname(splittedTocPath);

        if (!fs.existsSync(splittedTocDir)) {
            fs.mkdirsSync(splittedTocDir);
        }

        let splittedTocItem = {
            "uid": ns.uid,
            "name": ns.name,
            "items": ns.items
        }
        let splitTocModel = [splittedTocItem];
        let splitTocContent = yaml.safeDump(splitTocModel, config.yamlOption);
        fs.writeFileSync(splittedTocPath, splitTocContent, "utf8");
        console.log(`create new splitted toc (${splittedTocPath})`);

        let mergedTocItem = {
            "uid": ns.uid,
            "name": ns.name,
            "href": path.dirname(splittedTocRelativePath).replace("\\", "/") + "/"
        }

        mergedTocModel.push(mergedTocItem);
    });

    let mergedTocContent = yaml.safeDump(mergedTocModel, config.yamlOption);
    fs.writeFileSync(originalTocPath, mergedTocContent, "utf8");
    console.log(`rewrite original toc file (${originalTocPath})`);
}