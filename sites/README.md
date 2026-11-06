# Qolo Football site

Responsive static project site for Qolo Football. It uses plain HTML, CSS and JavaScript, with no build step. The site reports the actual foundation-stage status and does not claim that gameplay or Android builds exist.

## Local preview

From the repository root, run `python3 -m http.server 8000 --directory sites` and open `http://localhost:8000`.

## Vercel

Import the repository in Vercel and set the project Root Directory to `sites`. Use Framework Preset `Other`; leave Build Command and Output Directory blank. The static entry point is `index.html`. `vercel.json` applies clean URLs. No Vercel project has been configured or deployed by this source scaffold.
